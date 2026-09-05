using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    public class OcrPageImage : BaseEntity
    {
        public byte[] WholeImage { get; set; }

        public OcrPageImage(byte[] wholeImage)
            : base("WholeImage")
        {
            WholeImage = wholeImage;
        }
    }
}
