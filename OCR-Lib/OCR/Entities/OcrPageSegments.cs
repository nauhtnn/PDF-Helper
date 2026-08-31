using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR_Lib
{
    public class OcrPageSegments : OcrPageData
    {
        public List<byte[]> Segments { get; private set; }
        public OcrPageSegments()
        {
            RepresentationType = "Segments";

            Segments = new List<byte[]>();
        }

        public void AddSegment(byte[] segment)
        {
            Segments.Add(segment);
        }
    }
}
