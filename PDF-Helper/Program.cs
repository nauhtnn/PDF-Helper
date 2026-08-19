using OpenCvSharp;
using PDFtoImage;
using System;
using System.IO;
using Tesseract;

namespace PDF_Helper
{
    internal class Program
    {
        

        static void Main(string[] args)
        {
            string pdfPath = "E:\\Dev\\PDF-Helper\\sample_vietnamese.pdf";
            OCR(pdfPath);
        }
        static void OCR(string filePath)
        {
            var images = PNG_Converter.LoadFileForOCR(filePath);
            var resultText = new System.Text.StringBuilder();
            int pageIndex = 0;
            foreach (var image in images)
            {
                // Initialize Tesseract with Vietnamese traineddata
                using (var engine = new TesseractEngine(@"./tessdata", "vie", EngineMode.Default))
                {
                    using (var pix = Pix.LoadFromMemory(image))
                    {
                        using (var page = engine.Process(pix))
                        {
                            resultText.AppendLine(page.GetText());
                            Console.WriteLine("Recognized page {1} of file {0}.:", filePath, ++pageIndex);
                        }
                    }
                }
            }

            var outputFilePath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + "_OCR.txt");
            File.WriteAllText(outputFilePath, resultText.ToString(), System.Text.Encoding.UTF8);
        }
    }
}
