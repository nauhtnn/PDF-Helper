using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tesseract;

namespace OCR_Lib
{
    public sealed class OCR_Helper : DocumentProcessor
    {
        public static OCR_Helper instance;

        public static OCR_Helper GetInstance()
        {
            if (instance == null)
            {
                instance = new OCR_Helper();
            }
            return instance;
        }

        public OCR_Helper()
        {
            FileTypes = new string[] { "*.pdf" };
            //FileTypes = new string[] { "*.png", "*.jpg", "*.jpeg", "*.tif", "*.tiff" };
        }

        protected override void ProcessFileCore(string filePath)
        {
            if (StopRequested)
            {
                return;
            }

            StatusMessage.GetInstance().AddMessage($"Bắt đầu đọc file: {filePath}.");

            var images = PNG_Converter.LoadFileForOCR(filePath);
            var resultText = new System.Text.StringBuilder();
            int pageIndex = 0;
            foreach (var image in images)
            {
                if (StopRequested)
                {
                    return;
                }

                // Initialize Tesseract with Vietnamese traineddata
                using (var engine = new TesseractEngine(@"./", "vie", EngineMode.Default))
                {
                    using (var pix = Pix.LoadFromMemory(image))
                    {
                        using (var page = engine.Process(pix))
                        {
                            resultText.AppendLine(page.GetText());
                            StatusMessage.GetInstance().AddMessage($"Nhận dạng ký tự trang {++pageIndex}.");
                        }
                    }
                }
            }

            var outputFilePath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + "_OCR.txt");
            File.WriteAllText(outputFilePath, resultText.ToString(), System.Text.Encoding.UTF8);

            StatusMessage.GetInstance().AddMessage($"Hoàn thành đọc file: {filePath}.");
        }
    }
}
