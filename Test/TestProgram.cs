using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PdfLib;
using System.IO;

namespace Test
{
    class TestProgram
    {
        static void Main(string[] args)
        {
            TextCleaningStage stage = new TextCleaningStage();

            Console.WriteLine("Input file path:");

            string filePath = Console.ReadLine();

            LineFragment fragment = new LineFragment(File.ReadAllLines(filePath));

            DocumentList documents = stage.Execute(fragment) as DocumentList;

            SentenceSplittingStage x = new SentenceSplittingStage();
            x.Execute(documents);

            FileStream fileStream = File.OpenWrite("test.txt");

            /*int docIndex = 0;

            foreach (var d in documents.Documents)
            {
                docIndex++;
                byte[] textInBytes = Encoding.UTF8.GetBytes("\n<DOCUMENT " + docIndex + " />\n");
                fileStream.Write(textInBytes, 0, textInBytes.Length);

                foreach (var p in d.TextBlock)
                {
                    textInBytes = Encoding.UTF8.GetBytes(p + "\n");
                    fileStream.Write(textInBytes, 0, textInBytes.Length);
                }
            }*/

            LeaveSlipRuleStage leaveSlipRuleStage = new LeaveSlipRuleStage();

            leaveSlipRuleStage.ResetDefaults();
            DocumentList leaveSlips = leaveSlipRuleStage.Execute(documents) as DocumentList;

            foreach(var d in leaveSlips.Documents)
            {
                LeaveSlip leaveSlip = d as LeaveSlip;
                string p = leaveSlip.ToString();
                byte[] textInBytes = Encoding.UTF8.GetBytes(p + "\n");
                fileStream.Write(textInBytes, 0, textInBytes.Length);
            }

            fileStream.Flush();
            fileStream.Close();
        }
    }
}
