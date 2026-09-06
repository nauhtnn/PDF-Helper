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

        public LeaveSlip(Document doc) : base()
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

        public void AddUndefinedDate(string date)
        {
            UndefinedDates.Add(date);
        }

        public void AddEmployeeName(string name)
        {
            EmployeeName = name;
        }

        public void AddBossName(string name)
        {
            BossName = name;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Giấy nghỉ phép:");
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
