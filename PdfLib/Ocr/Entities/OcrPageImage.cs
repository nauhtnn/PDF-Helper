using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcrLib
{
    public class OcrPageImage : OcrPageData
    {
        public byte[] WholeImage { get; set; }

        public OcrPageImage(byte[] wholeImage)
        {
            RepresentationType = "WholeImage";

            WholeImage = wholeImage;
        }
    }
}
