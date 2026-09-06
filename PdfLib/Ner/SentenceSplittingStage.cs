using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PdfLib
{
    public class SentenceSplittingStage : PipelineStage
    {
        public SentenceSplittingStage()
            : base("Sentence Splitting", "Splits text into individual sentences") { }

        public override BaseEntity Execute(BaseEntity input)
        {
            Paragraph paragraphs = input as Paragraph;

            if (paragraphs == null)
            {
                throw new ArgumentException("Input must be of type Paragraph", nameof(input));
            }

            List<string> sentences = new List<string>();

            foreach (string paragraph in paragraphs.Paragraphs)
            {
                string pattern = @"\b(KT\.|TL\.) (?=[A-Z])";

                // Replace with escaped version: <Mr_>, <Mrs_>, etc.
                string escapedParagraph = Regex.Replace(paragraph, pattern, m =>
                {
                    string word = m.Groups[1].Value.Replace(".", "_");
                    return $"<{word}> ";
                });

                //no colon, semicolon in the split delimiters
                var rawSentence = escapedParagraph.Split(new[] { "./.", "...", "..", ".", "!", "?" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var sentence in rawSentence)
                {
                    string trimmedSentence = sentence.Trim();
                    if (!string.IsNullOrEmpty(trimmedSentence))
                    {
                        trimmedSentence = trimmedSentence.Replace("<KT_>", "KT.").Replace("<TL_>", "TL.");
                        sentences.Add(trimmedSentence);
                    }
                }
            }

            return new Sentence(sentences);
        }
    }
}
