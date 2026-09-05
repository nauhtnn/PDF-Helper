using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    public static class ProcessorFactory
    {
        public static DocumentProcessor CreateProcessor(string processorType)
        {
            switch (processorType)
            {
                case "OCR":
                    return OcrHelper.Instance;
                case "NER":
                    return NerLeaveSlip.Instance;
                default:
                    throw new ArgumentException($"Unknown processor type: {processorType}");
            }
        }
    }
}
