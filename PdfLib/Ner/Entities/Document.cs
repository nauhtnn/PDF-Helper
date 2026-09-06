using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    public class Document : BaseEntity
    {
        public List<string> TextBlock;

        public Document()
            : base("Document")
        {
            TextBlock = new List<string>();
        }
    }
}
