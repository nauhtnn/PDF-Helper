using System;
using System.IO;
using OCR_Lib;

namespace PDF_Helper_CLI
{
    internal class Program
    {        
        static void Main(string[] args)
        {
            string pdfPath = args.Length > 0 ? args[0] : "E:\\Dev\\PDF-Helper\\sample_vietnamese.pdf";
            OCR_Helper.OCR(pdfPath);
        }
    }
}
