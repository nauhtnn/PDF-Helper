using System;
using OpenCvSharp;
using Tesseract;

namespace OCR_Helper
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // Load image with OpenCV
            Mat image = Cv2.ImRead("D:\\Thuan\\OCR-Helper\\sample_vietnamese.png", ImreadModes.Grayscale);

            // Preprocess: thresholding for better OCR
            Cv2.Threshold(image, image, 0, 255, ThresholdTypes.Otsu);

            // Save preprocessed image (optional)
            Cv2.ImWrite("processed.png", image);

            // Initialize Tesseract with Vietnamese traineddata
            using (var engine = new TesseractEngine(@"./tessdata", "vie", EngineMode.Default))
            {
                using (var pix = Pix.LoadFromFile("processed.png"))
                {
                    using (var page = engine.Process(pix))
                    {
                        string text = page.GetText();
                        Console.WriteLine("Recognized Text:");
                        Console.WriteLine(text);
                    }
                }
            }
        }
    }
}
