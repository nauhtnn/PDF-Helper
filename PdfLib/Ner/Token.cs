using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    public class Token
    {
        // The raw text of the token
        public string Text { get; set; }

        // Part-of-Speech tag (e.g., Noun, Verb, Adjective)
        public string PosTag { get; set; }

        // Named Entity Recognition type (e.g., PER, LOC, ORG, DATE, or custom like DocumentType)
        public string NerTag { get; set; }

        // IOB boundary marker (B, I, O)
        public string IobTag { get; set; }

        // Combined tag (e.g., B-PER, I-LOC, O)
        public string IobNerTag
        {
            get
            {
                if (IobTag == "O" || string.IsNullOrEmpty(NerTag))
                    return "O";
                return $"{IobTag}-{NerTag}";
            }
        }

        public List<string> ConflictHistory { get; set; } = new List<string>();
        public string FinalLabel { get; set; }

        // Optional: position in sentence or document
        public int Index { get; set; }

        public Token(string text, string posTag, string nerTag, string iobTag, int index)
        {
            Text = text;
            PosTag = posTag;
            NerTag = nerTag;
            IobTag = iobTag;
            Index = index;
        }

        public override string ToString()
        {
            return $"{Index}: {Text} | POS={PosTag}, NER={NerTag}, IOB={IobTag}, Combined={IobNerTag}";
        }

        public void AddConflict(string label)
        {
            ConflictHistory.Add(label);
        }
    }

}
