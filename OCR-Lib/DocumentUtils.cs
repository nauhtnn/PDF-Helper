using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR_Lib
{
    public static class DocumentUtils
    {
        public static bool IsFirstPageOfDocument(List<string> lines, out DocumentType documentType)
        {
            documentType = DocumentType.Other;
            var line = lines.GetEnumerator();
            while (line.MoveNext())
            {
                var currentLine = TextMeasurement.RemoveAccent(line.Current);
                foreach (var mapping in DocumentTypeMapping.Values)
                {
                    if (currentLine.Equals(mapping.Value))
                    {
                        documentType = mapping.Key;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
