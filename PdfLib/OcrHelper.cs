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
            var resultFiles = new List<string>();
            var resultPageIndexMap = new Dictionary<int, string>();
            string resultPageIndex = "";
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

                        resultPageIndexMap.Add(resultFiles.Count - 1, resultPageIndex);
                        resultPageIndex = "";
                    }
                }

                joinedPageText.AppendLine(pageText.ToString());

                ++pageIndex;

                resultPageIndex += $"-{pageIndex}";

                StatusMessage.Instance.AddMessage($"Nhận dạng ký tự trang {pageIndex}.");
            }

            if (joinedPageText.Length > 0)
            {
                resultFiles.Add(joinedPageText.ToString());
                joinedPageText.Clear();

                resultPageIndexMap.Add(resultFiles.Count - 1, resultPageIndex);
                resultPageIndex = "";
            }

            string saveDirectory = Path.GetDirectoryName(filePath);
            string saveBaseName = Path.GetFileNameWithoutExtension(filePath) + ".txt";
            string saveAllBaseName = "";
            string saveAllFilePath = "";
            FileStream fileStream = null;
            int fileIndex = 0;
            bool saveAllEnabled = resultFiles.Count > 1;
            if(saveAllEnabled)
            {
                saveAllBaseName = Path.GetFileNameWithoutExtension(filePath) + ".txt.all";
                saveAllFilePath = PathHelper.Instance.GenerateFile(saveAllBaseName, saveDirectory, "_OCR");
                fileStream = File.OpenWrite(saveAllFilePath);
            }

            foreach (var fileContent in resultFiles)
            {
                var outputFilePath = PathHelper.Instance.GenerateFile(saveBaseName, saveDirectory, "_OCR");
                File.WriteAllText(outputFilePath, fileContent, System.Text.Encoding.UTF8);

                if(saveAllEnabled)
                {
                    // Write page index and file index (ALL UPPPERCASED) to the combined file
                    byte[] textInBytes = Encoding.UTF8.GetBytes($"\nTRANG {resultPageIndexMap[fileIndex]}: FILE SỐ {++fileIndex}\n\n");
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
