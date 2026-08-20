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
                _OCR_File(path);
            }
            else if (Directory.Exists(path))
            {
                _OCR_Folder(path, recursive: true);
            }
            else
            {
                StatusMessage.GetInstance().AddMessage("Không xử lý được ! Vui lòng cung cấp đường dẫn tới file PDF hoặc thư mục.");
            }
        }

        public static void OCR_Folder(string folderPath, bool recursive = false)
        {
            if (!File.Exists(folderPath))
            {
                StatusMessage.GetInstance().AddMessage(folderPath + " không phải là đường dẫn của một thư mục !");
                return;
            }

            _OCR_Folder(folderPath, recursive);
        }

        static void _OCR_Folder(string folderPath, bool recursive = false)
        {
            var pdfFiles = Directory.GetFiles(folderPath, "*.pdf",
                recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            if(pdfFiles.Count() > 0)
            {
                StatusMessage.GetInstance().AddMessage($"Bắt đầu đọc thư mục: {folderPath}.");
            }

            foreach (var pdfFile in pdfFiles)
            {
                _OCR_File(pdfFile);
            }

            if (pdfFiles.Count() > 0)
            {
                StatusMessage.GetInstance().AddMessage($"Hoàn thành đọc thư mục: {folderPath}.");
            }
        }

        public static void OCR_File(string filePath)
        {
            if(!File.Exists(filePath))
            {
                StatusMessage.GetInstance().AddMessage(filePath + " không phải là đường dẫn của một file !");
                return;
            }
            _OCR_File(filePath);
        }

        static void _OCR_File(string filePath)
        {
            StatusMessage.GetInstance().AddMessage($"Bắt đầu đọc file: {filePath}.");

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
                            StatusMessage.GetInstance().AddMessage($"Nhận diện ký tự trang {++pageIndex}.");
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
