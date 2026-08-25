using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR_Lib
{
    public static class TextMeasurement
    {
        public static string RemoveAccent(string input)
        {
            StringBuilder NoAccent = new StringBuilder();
            foreach(char c in input.Normalize(NormalizationForm.FormD))
            {
                if(System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                {
                    NoAccent.Append(c);
                }
            }

            return NoAccent.ToString();
        }
    }
}
