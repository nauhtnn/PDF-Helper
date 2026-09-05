using PdfLib.Ner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PdfLib
{
    public class TextCleaningStage : PipelineStage
    {
        public TextCleaningStage()
            : base("Text Cleaning", "Cleans and normalizes text") { }

        bool HandlePrefixes(string[] prefixes, string line, StringBuilder paragraph, List<string> paragraphs, bool isSeparatedLineForPrefix)
        {
            string lineWithoutPrefix = null;
            foreach (var prefix in prefixes)
            {
                bool found;
                
                if (isSeparatedLineForPrefix)
                    found = !string.IsNullOrEmpty(lineWithoutPrefix = TextMeasurement.FuzzyRemovePrefixOrEmpty(line, prefix));
                else
                    found = TextMeasurement.FuzzyStartsWith(line, prefix);
                
                if (found)
                {
                    if (paragraph.Length > 0)
                    {
                        paragraphs.Add(paragraph.ToString());
                        paragraph.Clear();
                    }

                    if (isSeparatedLineForPrefix)
                    {
                        paragraphs.Add(prefix);
                        paragraphs.Add(lineWithoutPrefix);
                    }
                    else
                        paragraphs.Add(line);

                    return true;
                }
            }

            return false;
        }

        public override BaseEntity Execute(BaseEntity input)
        {
            LineFragment textFragment = input as LineFragment;
            
            if (textFragment == null)
            {
                throw new ArgumentException("Input must be of type LineFragment", nameof(input));
            }

            List<string> paragraphs = new List<string>();

            StringBuilder paragraph = new StringBuilder();

            int i = 0;
            bool isNewParagraph = true;
            while (i < textFragment.Lines.Length)
            {
                string line = textFragment.Lines[i].Trim();
                i++;
                
                if (line.Length == 0)
                    continue;

                //indicators for new paragraph
                if (DocumentTypeMapping.ParseUpperDocumentTypeLine(line) != DocumentType.Unknown)
                {
                    if (paragraph.Length > 0)
                    {
                        paragraphs.Add(paragraph.ToString());
                        paragraph.Clear();
                    }
                    paragraphs.Add(line);
                    isNewParagraph = true;
                    continue;
                }

                bool isHandled = HandlePrefixes(new string[] { "CONG HOA XA HOI CHU NGHIA VIET NAM",
                    "Doc lap - Tu do - Hanh phuc",
                    "Kinh gui:",
                    "QUYET DINH:",
                    "Noi nhan:",
                    "Luu:" }, line, paragraph, paragraphs,
                    isSeparatedLineForPrefix : true);

                if (isHandled)
                {
                    isNewParagraph = true;
                    continue;
                }   

                isHandled = HandlePrefixes(new string[] { "KT.", "TL." },
                    line, paragraph, paragraphs, isSeparatedLineForPrefix : false);

                if (isHandled)
                {
                    isNewParagraph = true;
                    continue;
                }
                
                if (!isNewParagraph)
                {
                    paragraph.Append(" ");
                }

                paragraph.Append(line);

                //indicator for end of paragraph
                //line ends with punctuation or is all uppercase
                if (!Regex.IsMatch(line, @"[+\-,\(/\\]$") && Regex.IsMatch(line, @"[^\w\s]$") ||
                    line == line.ToUpper())
                {
                    paragraphs.Add(paragraph.ToString());
                    paragraph.Clear();

                    isNewParagraph = true;
                    continue;
                }

                isNewParagraph = false;
            }

            return new Paragraph(paragraphs);
        }
    }
}
