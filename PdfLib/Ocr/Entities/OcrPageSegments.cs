using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    public class OcrPageSegments : BaseEntity
    {
        public List<byte[]> Segments { get; private set; }
        public OcrPageSegments()
            : base("Segments")
        {
            Segments = new List<byte[]>();
        }

        public void AddSegment(byte[] segment)
        {
            Segments.Add(segment);
        }
    }
}
