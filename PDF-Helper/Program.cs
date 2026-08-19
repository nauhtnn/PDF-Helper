using OpenCvSharp;
using PDFtoImage;
using System;
using System.IO;
using Tesseract;

namespace PDF_Helper
{
    internal class Program
    {
        static void Debug_SaveImageToFile(SkiaSharp.SKBitmap bitmap, string filePath)
        {
            using (var image = SkiaSharp.SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100))
            using (var stream = File.OpenWrite(filePath))
            {
                data.SaveTo(stream);
            }
        }
        static void Debug_SavePdfToImages(string pdfPath, string outputDir)
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
                    Debug_SaveImageToFile(bmp, filePath);
                }
            }

            Console.WriteLine("PDF conversion complete!");
        }

        static MemoryStream LoadPDFtoMemory(string pdfPath)
        {
            byte[] bytes = File.ReadAllBytes(pdfPath);
            var stream = new MemoryStream();
            using (SkiaSharp.SKBitmap bitmap = Conversion.ToImage(bytes, 0))
            using (var image = SkiaSharp.SKImage.FromBitmap(bitmap))
            using (var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100))
            {
                data.SaveTo(stream);
            }
            return stream;
        }

        static void Main(string[] args)
        {
            string pdfPath = "E:\\Dev\\PDF-Helper\\sample_vietnamese.pdf";
#if DEBUG
            string pngPath = "E:\\Dev\\PDF-Helper\\sample_vietnamese.png";
            string outputDir = "E:\\Dev\\PDF-Helper\\output_images";
            Debug_SavePdfToImages(pdfPath, outputDir);
            OCR(pngPath);
#else
            OCR(pdfPath);
#endif
        }
        static void OCR(string filePath)
        {
            Mat image;
            if (filePath.EndsWith(".png"))
            {
                image = Cv2.ImRead(filePath, ImreadModes.Grayscale);
            }
            else
            {
                byte[] imageBytes = LoadPDFtoMemory(filePath).ToArray();

                // Decode image from byte array
                image = Cv2.ImDecode(imageBytes, ImreadModes.Grayscale);
            }

            // Preprocess: thresholding for better OCR
            Cv2.Threshold(image, image, 0, 255, ThresholdTypes.Otsu);

            // Save preprocessed image (optional)
#if DEBUG
            Cv2.ImWrite("processed.png", image);
#endif

            // Initialize Tesseract with Vietnamese traineddata
            using (var engine = new TesseractEngine(@"./tessdata", "vie", EngineMode.Default))
            {
#if DEBUG
                using (var pix = Pix.LoadFromFile("processed.png"))
#else

                using (var pix = Pix.LoadFromMemory(image.ToBytes(".png")))
#endif
                {
                    using (var page = engine.Process(pix))
                    {
                        string text = page.GetText();
                        Console.WriteLine("Recognized Text:");
                        Console.WriteLine(text);
                        File.WriteAllText("Vi-OCR.txt", text, System.Text.Encoding.UTF8);
                    }
                }
            }
        }
    }
}
