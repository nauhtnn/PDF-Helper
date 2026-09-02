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
            DebugContours(image, PathHelper.GenerateLocalFile("contours.png"));
            Cv2.ImWrite(PathHelper.GenerateLocalFile("processed.png"), image);
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
                if (OcrSettings.GetInstance().IsLineRemovalEnabled)
                    image = RemoveHorizontalLines(image);
                images.Add(image);
            }
            return images;
        }

        public static Mat RemoveHorizontalLines(Mat src,
            RetrievalModes retrievalMode = RetrievalModes.List,
            ContourApproximationModes approxMode = ContourApproximationModes.ApproxNone)
        {
            // Wide horizontal kernel
            int horizontalSize = OcrSettings.GetInstance().ColumnProjectionThreshold;
            Mat horizontalStructure = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(horizontalSize, 1));

            // Detect horizontal candidates
            Mat detectedLines = new Mat();
            Cv2.Erode(src, detectedLines, horizontalStructure);
            Cv2.Dilate(detectedLines, detectedLines, horizontalStructure);

            // Find contours of detected lines
            Cv2.FindContours(detectedLines, out Point[][] contours, out HierarchyIndex[] hierarchy,
                             retrievalMode, approxMode);
#if DEBUG
            DebugContours(detectedLines, PathHelper.GenerateLocalFile("contours.png"));
#endif

            // Build mask only for the thin line
            Mat mask = Mat.Zeros(src.Size(), MatType.CV_8UC1);
            foreach (var contour in contours)
            {
                Rect rect = Cv2.BoundingRect(contour);

                // Keep only objects with width ~240 px and height ~3 px
                if (rect.Width >= OcrSettings.GetInstance().MinColumnWidth * 3 && rect.Height <= OcrSettings.GetInstance().LineProjectionThreshold)
                {
                    Cv2.DrawContours(mask, new[] { contour }, -1, Scalar.White, -1);
                }
            }

            int lineRemovalMode = 0;

            if(lineRemovalMode == 0)
            {
#if DEBUG
                Cv2.ImWrite(PathHelper.GenerateLocalFile("detectedLines.png"), mask);
#endif
                // Fill the masked area with white directly
                src.SetTo(Scalar.White, mask);
                return src;
            }
            else if (lineRemovalMode == 1)
            {
                Cv2.BitwiseNot(mask, mask);
#if DEBUG
                Cv2.ImWrite(PathHelper.GenerateLocalFile("detectedLines.png"), mask);
#endif
                Mat result = new Mat();
                Cv2.BitwiseAnd(src, src, result, mask);
                return result;
            }
            else
            {
                // Inpaint to remove the line
                //Mat result = new Mat();
                //Cv2.Inpaint(src, mask, result, 3, InpaintMethod.Telea);
                return null;//This idea is under construction. It has not been finished.
            }
        }

#if DEBUG
        public static void DebugContours(Mat src, string debugPath,
            RetrievalModes retrievalMode = RetrievalModes.List,
            ContourApproximationModes approxMode = ContourApproximationModes.ApproxNone)
        {
            // src is already binary thresholded

            int horizontalSize = src.Cols / 30;
            Mat horizontalStructure = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(horizontalSize, 1));

            // Detect horizontal candidates
            Mat detectedLines = new Mat();
            Cv2.Erode(src, detectedLines, horizontalStructure);
            Cv2.Dilate(detectedLines, detectedLines, horizontalStructure);

            // Find contours
            Cv2.FindContours(detectedLines, out Point[][] contours, out HierarchyIndex[] hierarchy,
                             retrievalMode, approxMode);

            // Convert to BGR for colored drawing
            Mat debugImage = new Mat();
            Cv2.CvtColor(src, debugImage, ColorConversionCodes.GRAY2BGR);

            // Draw bounding boxes
            foreach (var contour in contours)
            {
                Rect rect = Cv2.BoundingRect(contour);
                Cv2.Rectangle(debugImage, rect, new Scalar(0, 0, 255), 2); // red box
            }

            // Save debug image
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
