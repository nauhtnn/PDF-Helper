using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcrLib
{
    class PredefinedPayroll
    {
        public static PredefinedPayroll instance;

        public static PredefinedPayroll GetInstance()
        {
            if (instance == null)
            {
                instance = new PredefinedPayroll();
            }
            return instance;
        }

        List<string> EmployeeNames;

        public PredefinedPayroll()
        {
            EmployeeNames = new List<string>();
        }

        public void AddName(string name)
        {
            EmployeeNames.Add(name);
        }

        public void AddNames(string filePath)
        {
            string[] lines = System.IO.File.ReadAllLines(filePath);
            EmployeeNames.AddRange(lines);
        }

        public List<string> GetOccurrences(string sentence)
        {
            if(EmployeeNames.Count == 0 && System.IO.File.Exists("PredefinedPayroll.txt"))
            {
                AddNames("PredefinedPayroll.txt");
            }

            List<string> occurrences = new List<string>();

            foreach (string name in EmployeeNames)
            {
                if (sentence.Contains(name))
                {
                    occurrences.Add(name);
                }
            }
            return occurrences;
        }
    }
}
