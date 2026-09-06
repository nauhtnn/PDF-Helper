using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    public class PathHelper
    {
        static PathHelper _instance;

        const int MaxFilesInLocalFolder = 200;

        const int MaxCounterToCheckMaxFiles = 20;

        int _counterToCheckMaxFiles;

        public static PathHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PathHelper();
                return _instance;
            }
        }

        PathHelper()
        {
            CleanLocalFolder();

            _counterToCheckMaxFiles = 0;
        }

        public static readonly string AppName = "Helper";

        void CleanLocalFolder()
        {
            string localFolder = LocalFolder();
            if (!Directory.Exists(localFolder))
                return;

            var files = Directory.GetFiles(localFolder);

            if(files.Count() <= 200)
                return;

            files = files.OrderBy(f => new FileInfo(f).CreationTime).ToArray();
            for(int i = 0; i < files.Length - 200; ++i)
            {
                try
                {
                    File.Delete(files[i]);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to delete file {files[i]}: {ex.Message}");
                }
            }
        }

        public string LocalFolder()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // Create a subfolder for your app
            string localFolder = Path.Combine(localAppData, AppName);
            if(!Directory.Exists(localFolder))
                Directory.CreateDirectory(localFolder);
            return localFolder;
        }

        public string GenerateLocalFile(string fileName)
        {
            _counterToCheckMaxFiles++;

            if(_counterToCheckMaxFiles > MaxCounterToCheckMaxFiles)
            {
                CleanLocalFolder();
                _counterToCheckMaxFiles = 0;
            }

            return GenerateFile(fileName, LocalFolder());
        }

        public string GenerateFile(string fileName, string directory, string postfix = "")
        {
            string baseFileName = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);

            string newFilePath = Path.Combine(directory, baseFileName + postfix + extension);

            if (!File.Exists(newFilePath))
                return newFilePath;

            for (int i = 0; i < int.MaxValue; ++i)
            {
                newFilePath = Path.Combine(directory, baseFileName + "-" + i + postfix + extension);
                if (!File.Exists(newFilePath))
                    return newFilePath;
            }

            throw new IndexOutOfRangeException();
        }
    }
}
