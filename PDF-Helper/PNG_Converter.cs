using OpenCvSharp;
using PDFtoImage;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace PDF_Helper
{
    internal class PNG_Converter
    {

        static List<MemoryStream> LoadPDF_toMemory(string pdfPath)
        {
            var streams = new List<MemoryStream>();
            byte[] bytes = File.ReadAllBytes(pdfPath);
            var pageCount = Conversion.GetPageCount(bytes);
            foreach(var page in Enumerable.Range(0, pageCount))
            {
                var stream = new MemoryStream();
                using (SkiaSharp.SKBitmap bitmap = Conversion.ToImage(bytes, page))
                using (var image = SkiaSharp.SKImage.FromBitmap(bitmap))
                using (var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100))
                {
                    data.SaveTo(stream);
                }
                streams.Add(stream);
            }
            
            return streams;
        }

        static void CvThreshold(Mat img)
        {
            Cv2.Threshold(img, img, 0, 255, ThresholdTypes.Otsu);
        }

        public static List<byte[]> LoadFileForOCR(string filePath)
        {
            if(System.IO.File.Exists(filePath) == false)
            {
                Console.WriteLine("File not found: {0}", filePath);
                return new List<byte[]>();  
            }

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

            return images.Select(img => img.ToBytes()).ToList();
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
                Mat image = Cv2.ImDecode(stream.ToArray(), ImreadModes.Grayscale);
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
