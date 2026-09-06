using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    public class Sentence : BaseEntity
    {
        public List<string> Sentences { get; private set; }

        public Sentence(List<string> sentences)
            : base("Sentence")
        {
            Sentences = sentences;
        }
    }
}
