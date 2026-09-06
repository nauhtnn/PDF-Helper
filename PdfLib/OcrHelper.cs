using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tesseract;

namespace PdfLib
{
    public sealed class OcrHelper : DocumentProcessor
    {
        static OcrHelper _instance;

        public static OcrHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new OcrHelper();
                }
                return _instance;
            }
        }

        public OcrHelper()
        {
            FileTypes = new string[] { "*.pdf", "*.png", "*.jpg", "*.jpeg", "*.tif", "*.tiff" };
        }

        protected override void ProcessFileCore(string filePath)
        {
            if (StopRequested)
            {
                return;
            }

            StatusMessage.Instance.AddMessage($"Bắt đầu đọc file: {filePath}.");

            var pages = PngConverter.LoadFileForOCR(filePath);
            var engine = new TesseractEngine(@"OCR\\", "vie", EngineMode.Default);
            var textOfPages = new List<string>();
            var pageText = new StringBuilder();
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

                textOfPages.Add(pageText.ToString());

                StatusMessage.Instance.AddMessage($"Nhận dạng ký tự trang {++pageIndex}.");
            }

            string saveDirectory = Path.GetDirectoryName(filePath);
            string saveBaseName = Path.GetFileNameWithoutExtension(filePath) + ".txt";
            string saveAllBaseName = "";
            string saveAllFilePath = "";
            FileStream fileStream = null;
            pageIndex = 0;
            bool saveAllEnabled = textOfPages.Count > 0;
            if(saveAllEnabled)
            {
                saveAllBaseName = Path.GetFileNameWithoutExtension(filePath) + ".txt.all";
                saveAllFilePath = PathHelper.Instance.GenerateFile(saveAllBaseName, saveDirectory, "_OCR");
                fileStream = File.OpenWrite(saveAllFilePath);
            }

            foreach (var fileContent in textOfPages)
            {
                //var outputFilePath = PathHelper.Instance.GenerateFile(saveBaseName, saveDirectory, "_OCR");
                //File.WriteAllText(outputFilePath, fileContent, System.Text.Encoding.UTF8);

                if(saveAllEnabled)
                {
                    // Write page index and file index (ALL IS UPPPERCASED) to the combined file
                    byte[] textInBytes = Encoding.UTF8.GetBytes($"\n<TRANG {++pageIndex} />\n\n");
                    fileStream.Write(textInBytes, 0, textInBytes.Length);

                    textInBytes = Encoding.UTF8.GetBytes(fileContent);
                    fileStream.Write(textInBytes, 0, textInBytes.Length);
                }
            }

            if(fileStream != null)
            {
                fileStream.Close();
            }

            StatusMessage.Instance.AddMessage($"Hoàn thành đọc file: {filePath}.");
        }
    }
}
