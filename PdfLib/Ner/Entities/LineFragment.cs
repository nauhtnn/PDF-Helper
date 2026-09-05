using PdfLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    public class LineFragment: BaseEntity
    {
        public string[] Lines { get; private set; }

        public LineFragment(string[] lines)
            : base("LineFragment")
        {
            Lines = lines;
        }
    }
}
