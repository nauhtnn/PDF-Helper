using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR_Lib
{
    public static class PathHelper
    {
        public static string AppName = "Helper";

        public static string LocalFolder()
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            // Create a subfolder for your app
            string localFolder = Path.Combine(localAppData, AppName);
            if(!Directory.Exists(localFolder))
                Directory.CreateDirectory(localFolder);
            return localFolder;
        }

        public static string GenerateLocalFile(string fileName)
        {
            string newFilePath = Path.Combine(LocalFolder(), fileName);

            if (!File.Exists(newFilePath))
                return newFilePath;

            string localFolder = LocalFolder();
            string baseFileName = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);

            for(int i = 0; i < int.MaxValue; ++i)
            {
                newFilePath = Path.Combine(LocalFolder(), baseFileName + "-" + i + extension);
                if (!File.Exists(newFilePath))
                    return newFilePath;
            }

            throw new IndexOutOfRangeException();
        }
    }
}
