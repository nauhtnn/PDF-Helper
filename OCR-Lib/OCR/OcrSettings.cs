using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR_Lib
{
    public class OcrSettings
    {
        // Singleton field
        private static OcrSettings _instance;

        // Singleton accessor method
        public static OcrSettings GetInstance()
        {
            if (_instance == null)
                _instance = new OcrSettings();
            return _instance;
        }

        // Properties
        public int LineProjectionThreshold { get; private set; }
        public int ColumnProjectionThreshold { get; private set; }
        public int MinLineHeight { get; private set; }
        public int MinColumnWidth { get; private set; }
        public bool IsGaussianBlurEnabled { get; private set; }
        public bool IsLineRemovalEnabled { get; private set; }

        OcrSettings()
        {
            try
            {
                if (!Load("OCR\\OcrSettings.txt"))
                    SetDefaultSettings();
            }
            catch
            {
                SetDefaultSettings();
            }
        }

        void SetDefaultSettings()
        {
            LineProjectionThreshold = 9;
            ColumnProjectionThreshold = 99;
            MinLineHeight = 32;
            MinColumnWidth = 32;
            IsGaussianBlurEnabled = false;
            IsLineRemovalEnabled = true;
        }

        // Load method
        bool Load(string filePath)
        {
            if (!System.IO.File.Exists(filePath))
                return false;

            var flag = 0x0;

            foreach (var line in System.IO.File.ReadAllLines(filePath))
            {
                var parts = line.Split('=');
                if (parts.Length != 2) continue;

                var key = parts[0].Trim();
                var value = parts[1].Trim();

                switch (key)
                {
                    case "LineProjectionThreshold":
                        LineProjectionThreshold = int.Parse(value);
                        flag |= 0x1;
                        break;
                    case "ColumnProjectionThreshold":
                        ColumnProjectionThreshold = int.Parse(value);
                        flag |= 0x10;
                        break;
                    case "MinLineHeight":
                        MinLineHeight = int.Parse(value);
                        flag |= 0x100;
                        break;
                    case "MinColumnWidth":
                        MinColumnWidth = int.Parse(value);
                        flag |= 0x1000;
                        break;
                    case "IsGaussianBlurEnabled":
                        IsGaussianBlurEnabled = int.Parse(value) > 0;
                        flag |= 0x10000;
                        break;
                    case "IsLineRemovalEnabled":
                        IsLineRemovalEnabled = int.Parse(value) > 0;
                        flag |= 0x100000;
                        break;
                }
            }

            return flag == 0x111111;
        }
    }
}
