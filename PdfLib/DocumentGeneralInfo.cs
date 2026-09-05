using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    public class DocumentGeneralInfo
    {
        public DocumentType DetectedDocType { get; private set; }
        public List<string> WrappedLines { get; private set; }
        public List<string> Sentences { get; private set; }

        public DocumentGeneralInfo()
        {
            DetectedDocType = DocumentType.Unknown;
            WrappedLines = new List<string>();
            Sentences = new List<string>();
        }

        public void Clear()
        {
            DetectedDocType = DocumentType.Unknown;
            WrappedLines.Clear();
            Sentences.Clear();
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
#if DEBUG
            string debugFilePath = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(filePath),
                System.IO.Path.GetFileName(filePath) + "_docGen_debug.txt");
            System.IO.File.WriteAllText(debugFilePath, string.Join(Environment.NewLine, WrappedLines));
#endif
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

        void TakeNonEmptyLines(string[] lines)
        {
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedLine))
                {
                    WrappedLines.Add(trimmedLine);
                    var detectedType = DocumentTypeMapping.ParseUpperDocumentTypeLine(trimmedLine);
                    if (detectedType != DocumentType.Unknown)
                    {
                        DetectedDocType = detectedType;
                    }
                }
            }
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
                else
                {
                    var detectedType = DocumentTypeMapping.ParseUpperDocumentTypeLine(line);
                    if (detectedType != DocumentType.Unknown)
                    {
                        DetectedDocType = detectedType;
                        WrappedLines.Add(joinedLine.ToString().Trim());
                        joinedLine.Clear();
                        WrappedLines.Add(line.Trim());
                    }
                    else
                    {
                        joinedLine.Append(line + " ");
                    }
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
