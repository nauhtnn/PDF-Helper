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
            if(OcrSettings.GetInstance().EnableGaussianBlur)
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

                    var segments = ImageSegmenter.GetInstance().SegmentByProjection(img);// SegmentImage(img);

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

        static List<Mat> SegmentImage(Mat image)
        {
            var segments = new List<Mat>();

            // Find contours
            Point[][] contours;
            HierarchyIndex[] hierarchy;
            Cv2.FindContours(image, out contours, out hierarchy,
                             RetrievalModes.External,
                             ContourApproximationModes.ApproxSimple);

            foreach (var contour in contours)
            {
                Rect rect = Cv2.BoundingRect(contour);

                // Filter out noise by size
                if (rect.Width > 20 && rect.Height > 10)
                {
                    Mat roi = new Mat(image, rect);
                    segments.Add(roi);
                }
            }

            return segments;
        }

        static Mat CvPreprocessImageFile(string filePath)
        {
            Mat image = Cv2.ImRead(filePath, ImreadModes.Grayscale);
            CvThreshold(image);
#if DEBUG
            Cv2.ImWrite("processed.png", image);
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
                images.Add(image);
            }
            return images;
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
    }
}
