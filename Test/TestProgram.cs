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

            Paragraph paragraphs = stage.Execute(fragment) as Paragraph;

            FileStream fileStream = File.OpenWrite("test.txt");

            foreach(var p in paragraphs.Paragraphs)
            {
                byte[] textInBytes = Encoding.UTF8.GetBytes(p + "\n");
                fileStream.Write(textInBytes, 0, textInBytes.Length);
            }

            fileStream.Flush();
            fileStream.Close();
        }
    }
}
