using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    public class SentenceSplittingStage : PipelineStage
    {
        public SentenceSplittingStage()
            : base("Sentence Splitting", "Splits text into individual sentences") { }

        public override BaseEntity Execute(BaseEntity input)
        {
            // TODO: Implement sentence splitting logic
            // Example: split by '.', '!', '?', etc.
            return input;
        }
    }
}
