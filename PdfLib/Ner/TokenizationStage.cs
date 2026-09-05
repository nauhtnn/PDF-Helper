using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    public class TokenizationStage : PipelineStage
    {
        public TokenizationStage()
            : base("Tokenization", "Splits sentences into tokens") { }

        public override BaseEntity Execute(BaseEntity input)
        {
            // TODO: Implement tokenization logic
            return input;
        }
    }
}
