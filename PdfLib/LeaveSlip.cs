using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    public class LeaveSlip : Document
    {
        public string RegNumber;

        public string PublishedDate;

        public string CommittingDate;
        public string StartLeaveDate;
        public string EndLeaveDate;
        public List<string> UndefinedDates;

        public int NumberOfLeaveDays;
        public string EmployeeName;

        public string BossName;

        public LeaveSlip() {
            Init();
        }

        public LeaveSlip(Document doc) : base(doc)
        {
            Init();
            if (doc != null)
            {
                TextBlock = doc.TextBlock;
            }
        }

        void Init()
        {
            RegNumber = string.Empty;
            PublishedDate = string.Empty;
            CommittingDate = string.Empty;
            StartLeaveDate = string.Empty;
            EndLeaveDate = string.Empty;
            UndefinedDates = new List<string>();
            EmployeeName = string.Empty;
            BossName = string.Empty;
            NumberOfLeaveDays = 0;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder docTypes = new StringBuilder();
            docTypes.Append("Loại văn bản: ");
            if(DocTypes.Count > 0)
            {
                for (int i = 0; i < DocTypes.Count; i++)
                {
                    docTypes.Append(DocumentTypeMapping.SentenceCase[DocTypes[i]]);
                    if (i < DocTypes.Count - 1)
                    {
                        docTypes.Append(", ");
                    }
                }
            }
            else
            {
                docTypes.Append("Không xác định");
            }
            sb.AppendLine(docTypes.ToString());
            sb.AppendLine("Số: " + RegNumber);
            sb.AppendLine("Ngày ban hành: " + PublishedDate);
            sb.AppendLine("Ngày nộp đơn: " + CommittingDate);
            sb.AppendLine("Ngày bắt đầu nghỉ: " + StartLeaveDate);
            sb.AppendLine("Ngày kết thúc nghỉ: " + EndLeaveDate);
            sb.AppendLine("Số ngày nghỉ: " + NumberOfLeaveDays);
            sb.AppendLine("Các ngày khác: " + string.Join(", ", UndefinedDates));
            sb.AppendLine("Tên người xin nghỉ phép: " + EmployeeName);
            sb.AppendLine("Tên cấp trên: " + BossName);
            return sb.ToString();
        }
    }
}
