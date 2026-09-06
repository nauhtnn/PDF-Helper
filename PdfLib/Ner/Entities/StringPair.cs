using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    public class StringPair
    {
        public string Input { get; set; }
        public string Output { get; set; }
        public StringPair(string input, string output)
        {
            Input = input;
            Output = output;
        }
    }
}
