using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR_Lib
{
    abstract public class DocumentProcessor
    {
        public string[] FileTypes { get; set; }
        public bool StopRequested { get; set; } = false;

        public void ProcessFolder(string folderPath, bool recursive = false)
        {

            string normalizedFolderPath;
            if (!folderPath.EndsWith(Path.DirectorySeparatorChar.ToString()))
            {
                normalizedFolderPath = folderPath + Path.DirectorySeparatorChar;
            }
            else
            {
                normalizedFolderPath = folderPath;
            }

            if (!Directory.Exists(normalizedFolderPath))
            {
                StatusMessage.GetInstance().AddMessage(normalizedFolderPath + " không phải là đường dẫn của một thư mục !");
                return;
            }

            ProcessFolderInternal(normalizedFolderPath, recursive);
        }

        protected void ProcessFolderInternal(string folderPath, bool recursive = false)
        {
            if (StopRequested)
            {
                return;
            }
            StatusMessage.GetInstance().AddMessage($"Bắt đầu đọc thư mục: {folderPath}.");
            List<string> sourceFiles = new List<string>();
            try
            {
                foreach(string fileType in FileTypes)
                {
                    var files = Directory.GetFiles(folderPath, fileType);
                    sourceFiles.AddRange(files);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                StatusMessage.GetInstance().AddMessage("Không có quyền đọc thư mục: " + ex.Message);
            }
            catch (Exception ex)
            {
                StatusMessage.GetInstance().AddMessage("Lỗi khi đọc thư mục: " + ex.Message);
            }
            if (sourceFiles != null)
            {
                foreach (var file in sourceFiles)
                {
                    if (StopRequested)
                    {
                        return;
                    }
                    ProcessFileCore(file);
                }
            }
            if (recursive)
            {
                string[] subDirectories = null;
                try
                {
                    subDirectories = Directory.GetDirectories(folderPath);
                }
                catch (UnauthorizedAccessException ex)
                {
                    StatusMessage.GetInstance().AddMessage("Không có quyền đọc thư mục con: " + ex.Message);
                }
                catch (Exception ex)
                {
                    StatusMessage.GetInstance().AddMessage("Lỗi khi đọc thư mục con: " + ex.Message);
                }
                if (subDirectories != null)
                {
                    foreach (var subDir in subDirectories)
                    {
                        if (StopRequested)
                        {
                            return;
                        }
                        ProcessFolderInternal(subDir, recursive);
                    }
                }
            }
        }

        public void ProcessFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                StatusMessage.GetInstance().AddMessage(filePath + " không phải là đường dẫn của một file !");
                return;
            }

            string fileExtension = Path.GetExtension(filePath);
            if(FileTypes != null && FileTypes.Length > 0)
            {
                bool isSupported = false;
                foreach (var fileType in FileTypes)
                {
                    if (fileType.StartsWith("*"))
                    {
                        string extension = fileType.Substring(1);
                        if (string.Equals(fileExtension, extension, StringComparison.OrdinalIgnoreCase))
                        {
                            isSupported = true;
                            break;
                        }
                    }
                }
                if (!isSupported)
                {
                    StatusMessage.GetInstance().AddMessage($"Tác vụ này không hỗ trợ định dạng file: {filePath} !");
                    return;
                }
            }

            ProcessFileCore(filePath);
        }

        protected abstract void ProcessFileCore(string filePath);
    }
}
