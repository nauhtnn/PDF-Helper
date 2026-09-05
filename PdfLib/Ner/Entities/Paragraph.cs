using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    public class Paragraph : BaseEntity
    {
        public List<string> Paragraphs { get; private set; }

        public Paragraph(List<string> paragraphs)
            : base("Paragraph")
        {
            Paragraphs = paragraphs;
        }
    }
}
