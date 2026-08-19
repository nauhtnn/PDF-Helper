using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Tesseract;

namespace OCR_Lib
{
    public class OCR_Helper
    {
        public static void OCR(string path)
        {
            if (File.Exists(path) && Path.GetExtension(path).Equals(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                OCR_File(path);
            }
            else if (Directory.Exists(path))
            {
                OCR_Folder(path, recursive: true);
                StatusMessage.GetInstance().AddMessage("Hoàn thành đọc thư mục " + path + ".");
            }
            else
            {
                StatusMessage.GetInstance().AddMessage("Không xử lý được. Vui lòng cung cấp đường dẫn tới file PDF hoặc thư mục.");
            }
        }

        static void OCR_Folder(string folderPath, bool recursive = false)
        {
            var pdfFiles = Directory.GetFiles(folderPath, "*.pdf",
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            foreach (var pdfFile in pdfFiles)
            {
                OCR_File(pdfFile);
            }
        }

        static void OCR_File(string filePath)
        {
            var images = PNG_Converter.LoadFileForOCR(filePath);
            var resultText = new System.Text.StringBuilder();
            int pageIndex = 0;
            foreach (var image in images)
            {
                // Initialize Tesseract with Vietnamese traineddata
                using (var engine = new TesseractEngine(@"./", "vie", EngineMode.Default))
                {
                    using (var pix = Pix.LoadFromMemory(image))
                    {
                        using (var page = engine.Process(pix))
                        {
                            resultText.AppendLine(page.GetText());
                            StatusMessage.GetInstance().AddMessage($"Nhận diện ký tự trang {++pageIndex} của file {filePath}.");
                        }
                    }
                }
            }

            var outputFilePath = Path.Combine(Path.GetDirectoryName(filePath), Path.GetFileNameWithoutExtension(filePath) + "_OCR.txt");
            File.WriteAllText(outputFilePath, resultText.ToString(), System.Text.Encoding.UTF8);
        }
    }
}
