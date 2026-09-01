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

            var pages = PNG_Converter.LoadFileForOCR(filePath);
            var engine = new TesseractEngine(@"OCR\\", "vie", EngineMode.Default);
            var resultFiles = new List<string>();
            var pageText = new StringBuilder();
            var joinedPageText = new StringBuilder();
            int pageIndex = 0;
            foreach (var page in pages)
            {
                pageText.Clear();

                if (StopRequested)
                {
                    return;
                }

                List<byte[]> data;
                if(page is OcrPageImage)
                {
                    data = new List<byte[]>();
                    data.Add((page as OcrPageImage).WholeImage);
                }
                else
                {
                    data = (page as OcrPageSegments).Segments;
                }

                foreach (var segment in data)
                {
                    using (var pix = Pix.LoadFromMemory(segment))
                    {
                        using (var t = engine.Process(pix))
                        {
                            pageText.AppendLine(t.GetText());
                        }
                    }
                }

                string[] lines = pageText.ToString().Split(new[] { '\n' });

                var docType = DocumentType.Unknown;

                foreach (var line in lines)
                {
                    var detectedType = DocumentTypeMapping.ParseUpperDocumentTypeLine(line);
                    if (detectedType != DocumentType.Unknown)
                    {
                        docType = detectedType;
                        break;
                    }
                }

                if (docType != DocumentType.Unknown)
                {
                    if (joinedPageText.Length > 0)
                    {
                        resultFiles.Add(joinedPageText.ToString());
                        joinedPageText.Clear();
                    }
                }

                joinedPageText.AppendLine(pageText.ToString());

                StatusMessage.GetInstance().AddMessage($"Nhận dạng ký tự trang {++pageIndex}.");
            }

            if (joinedPageText.Length > 0)
            {
                resultFiles.Add(joinedPageText.ToString());
                joinedPageText.Clear();
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
