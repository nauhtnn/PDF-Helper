using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR_Lib
{
    public class DocumentGeneralInfo
    {
        public DocumentType DocumentTypeSingleLine { get; private set; }
        public List<string> WrappedLines { get; private set; }
        public List<string> Sentences { get; private set; }

        public DocumentGeneralInfo()
        {
            DocumentTypeSingleLine = DocumentType.Other;
            WrappedLines = new List<string>();
            Sentences = new List<string>();
        }

        public void Clear()
        {
            DocumentTypeSingleLine = DocumentType.Other;
            WrappedLines.Clear();
            Sentences.Clear();
        }

        bool IsDocumentTypeSingleLine(string line)
        {
            var simpleLine = TextMeasurement.RemoveAccent(line);
            foreach (var mapping in DocumentTypeMapping.UpperUnmarked)
            {
                if (simpleLine.StartsWith(mapping.Value))
                {
                    DocumentTypeSingleLine = mapping.Key;
                    return true;
                }
            }
            return false;
        }

        void BuildSentences()
        {
            foreach (string line in WrappedLines)
            {
                // Simple sentence splitting logic - replace with actual NER sentence splitting as needed
                string[] subSentences = line.Split(new char[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string subSentence in subSentences)
                {
                    string trimmedSentence = subSentence.Trim();
                    if (!string.IsNullOrEmpty(trimmedSentence))
                    {
                        Sentences.Add(trimmedSentence);
                    }
                }
            }
        }

        public void ProcessFile(string filePath)
        {
            string[] lines = System.IO.File.ReadAllLines(filePath);
            ParseLines(lines);
        }

        public void ProcessText(string text)
        {
            string[] lines = text.Split(new[] { '\n' });
            ParseLines(lines);
        }

        void ParseLines(string[] lines)
        {
            BuildWrappedLines(lines);
            BuildSentences();
        }

        void BuildWrappedLines(string[] lines)
        {
            System.Text.StringBuilder joinedLine = new System.Text.StringBuilder();
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    WrappedLines.Add(joinedLine.ToString().Trim());
                    joinedLine.Clear();
                }
                else if(IsDocumentTypeSingleLine(line))
                {
                    WrappedLines.Add(joinedLine.ToString().Trim());
                    joinedLine.Clear();
                    WrappedLines.Add(line.Trim());
                }
                else
                {
                    joinedLine.Append(line + " ");
                }
            }

            // Add the last joined line if it has content
            if (joinedLine.Length > 0)
            {
                WrappedLines.Add(joinedLine.ToString().Trim());
            }
        }
    }
}
