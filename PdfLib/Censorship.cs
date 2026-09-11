using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PdfLib
{
    public class Censorship
    {
        static Censorship _instance;
        public static Censorship Instance
        {
            get {
                if(_instance == null)
                    _instance = new Censorship("BadWords.bin");
                return _instance;
            }
        }

        string _badWordsPattern = null;
        const byte _key = 0xAA; // secret XOR key

        public Censorship() { }

        public Censorship(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) ||
                !File.Exists(filePath))
                return;

            byte[] buffer = File.ReadAllBytes(filePath);

            if (buffer.Length > 0)
            {
                // Reverse bitwise obfuscation
                for (int i = 0; i < buffer.Length; i++)
                    buffer[i] ^= _key;

                _badWordsPattern = Encoding.UTF8.GetString(buffer);
            }
        }

        public string ScanAndReplace(string text)
        {
            if (string.IsNullOrEmpty(_badWordsPattern))
                return text;

            /*string censored = Regex.Replace(text, _badWordsPattern, "***", RegexOptions.IgnoreCase);
            censored = Regex.Replace(censored, @"\s{2,}", " ");
            return censored.Trim();*/
            return Regex.Replace(text, _badWordsPattern, "***", RegexOptions.IgnoreCase);
        }
    }
}
