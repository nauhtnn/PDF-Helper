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
                    _instance = new Censorship("BadWords.dat");
                return _instance;
            }
        }

        List<string> _badWords = new List<string>();

        public Censorship() { }

        public Censorship(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return;
            FileStream stream = File.OpenRead(filePath);
            byte[] buffer = new byte[stream.Length];
            stream.Read(buffer, 0, buffer.Length);
            stream.Close();
            int offset = 0;
            while(offset < buffer.Length - 4)
            {
                int stringSize = BitConverter.ToInt32(buffer, offset);
                offset += 4;
                string badWord = Encoding.UTF8.GetString(buffer, offset, stringSize);
                offset += stringSize;
                _badWords.Add(badWord);
            }
        }

        public string ScanAndReplace(string text)
        {
            if (_badWords.Count == 0)
                return text;
            
            StringBuilder patternBuilder = new StringBuilder();
            foreach (string word in _badWords)
                patternBuilder.Append(word + "|");

            if(patternBuilder.Length == 0)
                return text;

            patternBuilder.Remove(patternBuilder.Length - 1, 1);

            string patterns = patternBuilder.ToString();

            return Regex.Replace(text, "\b(" + patterns + ")\b", " ");
        }
    }
}
