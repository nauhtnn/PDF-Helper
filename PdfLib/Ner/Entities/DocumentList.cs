using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    public class DocumentList : BaseEntity
    {
        public List<Document> Documents { get; private set; }

        public DocumentList()
            : base("DocumentList")
        {
            Documents = new List<Document>();
        }

        public DocumentList(List<Document> documents)
            : base("DocumentList")
        {
            Documents = documents;
        }
    }
}