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

                //new paragraph indicators
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

                string[] specialStart = { "CONG HOA XA HOI CHU NGHIA VIET NAM",
                    "Doc lap - Tu do - Hanh phuc",
                    "Kinh gui:",
                    "QUYET DINH:",
                    "Noi nhan:",
                    "Luu:"
                };
                string noSpecialStart = null;
                bool hasSpecialWord = false;
                foreach (var start in specialStart)
                {
                    if ((noSpecialStart = TextMeasurement.FuzzryRemovePrefix(line, start)) != null)
                    {
                        if (paragraph.Length > 0)
                        {
                            paragraphs.Add(paragraph.ToString());
                            paragraph.Clear();
                        }
                        paragraphs.Add(start);
                        paragraphs.Add(noSpecialStart);
                        isNewParagraph = true;
                        hasSpecialWord = true;
                        break;
                    }
                }

                if(hasSpecialWord)
                {
                    continue;
                }

                specialStart = { "KT.", "TL." };

                string[] specialEnd = { "CONG HOA XA HOI CHU NGHIA VIET NAM",
                    "Doc lap - Tu do - Hanh phuc",
                    "Noi nhan:", "Kinh gui:", "KT.", "TL." };

                if ((noSpecialStart = TextMeasurement.FuzzryRemovePrefix(line, "Noi nhan:")) != null)
                {
                    if (paragraph.Length > 0)
                    {
                        paragraphs.Add(paragraph.ToString());
                        paragraph.Clear();
                    }
                    isNewParagraph = true;
                }
                else
                {
                    if (!isNewParagraph)
                    {
                        paragraph.Append(" ");
                    }

                    paragraph.Append(line);

                    if (Regex.IsMatch(line, @"[a-z0-9+\-,;\(/\\]$")) // Example regex for numbered lists
                    {
                        isNewParagraph = false;
                    }
                }
            }

            return new Paragraph(paragraphs);
        }
    }
}
