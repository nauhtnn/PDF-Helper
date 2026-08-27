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
            FileTypes = new string[] { "*.pdf", "*.png", "*.jpg", "*.jpeg", "*.tif", "*.tiff" };
        }

        protected override void ProcessFileCore(string filePath)
        {
            if (StopRequested)
            {
                return;
            }

            StatusMessage.GetInstance().AddMessage($"Bắt đầu đọc file: {filePath}.");

            var images = PNG_Converter.LoadFileForOCR(filePath);
            var engine = new TesseractEngine(@"./", "vie", EngineMode.Default);
            var documentInfo = new DocumentGeneralInfo();
            var resultFiles = new List<string>();
            var bufferedText = new System.Text.StringBuilder();
            int pageIndex = 0;
            foreach (var image in images)
            {
                if (StopRequested)
                {
                    return;
                }
                using (var pix = Pix.LoadFromMemory(image))
                {
                    using (var page = engine.Process(pix))
                    {
                        documentInfo.Clear();
                        documentInfo.ProcessText(page.GetText());

                        if(documentInfo.WrappedLines.Count == 0)
                        {
                            continue;
                        }

                        if (documentInfo.DocumentTypeSingleLine != DocumentType.Other)
                        {
                            if (bufferedText.Length > 0)
                            {
                                resultFiles.Add(bufferedText.ToString());
                                bufferedText.Clear();
                            }
                        }

                        foreach (var line in documentInfo.WrappedLines)
                        {
                            bufferedText.AppendLine(line);
                        }

                        StatusMessage.GetInstance().AddMessage($"Nhận dạng ký tự trang {++pageIndex}.");
                    }
                }
            }

            int fileIndex = 0;
            foreach(var fileContent in resultFiles)
            {
                var outputFilePath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + $"_{++fileIndex}_OCR.txt");
                File.WriteAllText(outputFilePath, fileContent, System.Text.Encoding.UTF8);
            }

            StatusMessage.GetInstance().AddMessage($"Hoàn thành đọc file: {filePath}.");
        }
    }
}
