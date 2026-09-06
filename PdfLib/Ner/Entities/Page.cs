using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    internal class Page
    {
        public List<string> Paragraphs { get; private set; }

        public Page()
        {
            Paragraphs = new List<string>();
        }
    }
}
