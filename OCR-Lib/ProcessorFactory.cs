using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR_Lib
{
    public static class ProcessorFactory
    {
        public static DocumentProcessor CreateProcessor(string processorType)
        {
            switch (processorType)
            {
                case "OCR":
                    return OCR_Helper.GetInstance();
                case "NER":
                    return NER_LeaveSlip.GetInstance();
                default:
                    throw new ArgumentException($"Unknown processor type: {processorType}");
            }
        }
    }
}
