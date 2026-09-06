using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    public class Document : BaseEntity
    {
        public List<DocumentType> DocTypes;
        public List<string> TextBlock;

        public Document()
            : base("Document")
        {
            DocTypes = new List<DocumentType>();
            TextBlock = new List<string>();
        }

        public Document(Document doc)
            : base("Document")
        {
            if (doc != null)
            {
                DocTypes = doc.DocTypes;
                TextBlock = doc.TextBlock;
            }
            else
            {
                DocTypes = new List<DocumentType>();
                TextBlock = new List<string>();
            }
        }
    }
}
