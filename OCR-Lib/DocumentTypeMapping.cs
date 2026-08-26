using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR_Lib
{
    public static class DocumentTypeMapping
    {
        public static readonly Dictionary<DocumentType, string> Values =
        new Dictionary<DocumentType, string>
        {
            { DocumentType.LeaveSlip, "GIAY NGHI PHEP" },
            { DocumentType.LeaveRequest, "DON XIN NGHI PHEP" },
            { DocumentType.Decision, "QUYET DINH" },
            { DocumentType.Announcement, "THONG BAO" }
        };
    }
}
