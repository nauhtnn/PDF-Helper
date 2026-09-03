using DocumentFormat.OpenXml.Spreadsheet;
using OpenCvSharp;
using PDFtoImage;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace OCR_Lib
{
    internal class PNG_Converter
    {

        static List<MemoryStream> LoadPDF_toMemory(string pdfPath)
        {
            var streams = new List<MemoryStream>();
            byte[] bytes = File.ReadAllBytes(pdfPath);
            var pageCount = Conversion.GetPageCount(bytes);
            int pageIndex = 0;
            while(pageIndex < pageCount)
            {
                var stream = new MemoryStream();
                using (SkiaSharp.SKBitmap bitmap = Conversion.ToImage(bytes, pageIndex))
                using (var image = SkiaSharp.SKImage.FromBitmap(bitmap))
                using (var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100))
                {
                    data.SaveTo(stream);
                }
                streams.Add(stream);
                StatusMessage.GetInstance().AddMessage($"Tải lên RAM trang {++pageIndex}.");
            }
            
            return streams;
        }

        static void CvThreshold(Mat img)
        {
            Cv2.Threshold(img, img, 0, 255, ThresholdTypes.Otsu);

            // Apply Gaussian blur
            if(OcrSettings.GetInstance().IsGaussianBlurEnabled)
                Cv2.GaussianBlur(img, img, new Size(5, 5), 0);
        }

        public static List<OcrPageData> LoadFileForOCR(string filePath, bool segment = true)
        {
            List<Mat> images;
            if(filePath.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                var streams = LoadPDF_toMemory(filePath);
                images = CvPreprocessMemoryStreams(streams);
            }
            else
            {
                images = new List<Mat> { CvPreprocessImageFile(filePath) };
            }

            if(segment)
            {
                var segmentedPages = new List<OcrPageData>();
                foreach (var img in images)
                {
                    var segmentedPage = new OcrPageSegments();

                    var segments = ImageSegmenter.GetInstance().SegmentByProjection(img);

                    foreach (var seg in segments)
                        segmentedPage.AddSegment(seg.ToBytes());

                    segmentedPages.Add(segmentedPage);
                }
                return segmentedPages;
            }

            var page = new List<OcrPageData>();
            foreach (var img in images)
                page.Add(new OcrPageImage(img.ToBytes()));
            return page;
        }

        static Mat CvPreprocessImageFile(string filePath)
        {
            Mat image = Cv2.ImRead(filePath, ImreadModes.Grayscale);
            CvThreshold(image);
            if (OcrSettings.GetInstance().IsLineRemovalEnabled)
                image = RemoveHorizontalLines(image);
#if DEBUG
            DebugContoursHierarchy(image, PathHelper.Instance.GenerateLocalFile("all_contours.png"));
            Cv2.ImWrite(PathHelper.Instance.GenerateLocalFile("processed.png"), image);
#endif
            return image;
        }

        static List<Mat> CvPreprocessMemoryStreams(List<MemoryStream> streams)
        {
            var images = new List<Mat>();

            foreach(var stream in streams)
            {
                Mat image = Cv2.ImDecode(stream.GetBuffer(), ImreadModes.Grayscale);
                CvThreshold(image);
#if DEBUG
                DebugContoursHierarchy(image, PathHelper.Instance.GenerateLocalFile("all_contours.png"));
#endif
                if (OcrSettings.GetInstance().IsLineRemovalEnabled)
                    image = RemoveHorizontalLines(image);
                images.Add(image);
#if DEBUG
                Cv2.ImWrite(PathHelper.Instance.GenerateLocalFile("processed.png"), image);
#endif
            }
            return images;
        }

        public static Mat RemoveHorizontalLines(Mat src)
        {
            // Invert the image to make lines white on black background
            using (Mat invertSrc = new Mat())
            using (Mat detectedLines = new Mat())
            {
                Cv2.BitwiseNot(src, invertSrc);

                Mat horizontalStructure = Cv2.GetStructuringElement(MorphShapes.Rect,
                new Size(OcrSettings.GetInstance().ColumnProjectionThreshold, 1));

                Cv2.Erode(invertSrc, detectedLines, horizontalStructure);
#if DEBUG
                Cv2.ImWrite(PathHelper.Instance.GenerateLocalFile("erosion_source.png"), detectedLines);
#endif
                Cv2.Dilate(detectedLines, detectedLines, horizontalStructure);
#if DEBUG
                Cv2.ImWrite(PathHelper.Instance.GenerateLocalFile("dilation_after_erosion.png"), detectedLines);
#endif
                // Fill the masked area with white directly
                src.SetTo(Scalar.White, detectedLines);
            }
            
            return src;
        }

        public static Mat RemoveHorizontalLines2(Mat src,
            RetrievalModes retrievalMode = RetrievalModes.Tree,
            ContourApproximationModes approxMode = ContourApproximationModes.ApproxNone)
        {
            // Find contours of detected lines
            Cv2.FindContours(src, out Point[][] contours, out HierarchyIndex[] hierarchy,
                             retrievalMode, approxMode);

            // Build mask only for the thin line
            Mat mask = Mat.Zeros(src.Size(), MatType.CV_8UC1);
            for (int i = 0; i < contours.Length; i++)
            {
                Rect rect = Cv2.BoundingRect(contours[i]);

                // Determine hierarchy depth
                int depth = 0;
                int parent = hierarchy[i].Parent;
                while (parent != -1 && depth < 2)
                {
                    depth++;
                    parent = hierarchy[parent].Parent;
                }

                if (rect.Width >= OcrSettings.GetInstance().MinColumnWidth * 3 && rect.Height <= OcrSettings.GetInstance().LineProjectionThreshold)
                {
                    Cv2.DrawContours(mask, new[] { contours[i] }, -1, Scalar.White, -1);
                }
            }

#if DEBUG
            Cv2.ImWrite(PathHelper.Instance.GenerateLocalFile("detectedLines.png"), mask);
#endif
            // Fill the masked area with white directly
            src.SetTo(Scalar.White, mask);
            return src;
        }

#if DEBUG
        public static void DebugContoursHierarchy(Mat binaryImage, string debugPath)
        {
            // Find contours with hierarchy
            Cv2.FindContours(binaryImage,
                out Point[][] contours,
                out HierarchyIndex[] hierarchy,
                RetrievalModes.Tree,
                ContourApproximationModes.ApproxSimple);

            // Convert to BGR for colored drawing
            Mat debugImage = new Mat();
            Cv2.CvtColor(binaryImage, debugImage, ColorConversionCodes.GRAY2BGR);

            for (int i = 0; i < contours.Length; i++)
            {
                Rect rect = Cv2.BoundingRect(contours[i]);

                // Determine hierarchy depth
                int depth = 0;
                int parent = hierarchy[i].Parent;
                while (parent != -1)
                {
                    depth++;
                    parent = hierarchy[parent].Parent;
                }

                // Choose color by depth
                Scalar color;
                if (depth == 0)       // parent
                    color = new Scalar(0, 0, 255);   // red
                else if (depth == 1)  // child
                    color = new Scalar(255, 0, 0);   // blue
                else if (depth == 2)  // grandchild
                    color = new Scalar(0, 255, 0);   // green
                else
                    color = new Scalar(0, 255, 255); // yellow for deeper levels

                Cv2.Rectangle(debugImage, rect, color, 2);
            }

            Cv2.ImWrite(debugPath, debugImage);
        }

        public static void Debug_SavePDF_to_PNG_File(string pdfPath, string outputDir)
        {
            Directory.CreateDirectory(outputDir);

            byte[] bytes = File.ReadAllBytes(pdfPath);

            int pageCount = Conversion.GetPageCount(bytes);
            for (int page = 0; page < pageCount; page++)
            {
                using (SkiaSharp.SKBitmap bmp = Conversion.ToImage(bytes, page))
                {
                    // Save to file
                    string filePath = Path.Combine(outputDir, $"page_{page + 1}.png");
                    using (var image = SkiaSharp.SKImage.FromBitmap(bmp))
                    using (var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100))
                    using (var stream = File.OpenWrite(filePath))
                    {
                        data.SaveTo(stream);
                    }

                    Console.WriteLine($"Saved to {filePath}");
                }
            }
        }
#endif
    }
}
