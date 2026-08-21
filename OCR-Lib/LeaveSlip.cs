using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OCR_Lib
{
    public class LeaveSlip
    {
        public string FilePath;

        public string CommittingDate;
        public string StartLeaveDate;
        public string EndLeaveDate;
        public List<string> UndefinedDates;

        public int NumberOfLeaveDays;
        public List<string> EmployeeNames;

        public LeaveSlip() {
            FilePath = string.Empty;
            UndefinedDates = new List<string>();
            EmployeeNames = new List<string>();
            NumberOfLeaveDays = 0;
        }

        public void AddUndefinedDate(string date)
        {
            UndefinedDates.Add(date);
        }

        public void AddEmployeeName(string name)
        {
            if(!EmployeeNames.Contains(name))
            {
                EmployeeNames.Add(name);
            }
        }

        public void AddEmployeeNames(IEnumerable<string> names)
        {
            foreach (string name in names)
            {
                if(!EmployeeNames.Contains(name))
                {
                    EmployeeNames.Add(name);
                }
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Giấy nghỉ phép:");
            sb.AppendLine("Đường dẫn File: " + FilePath);
            sb.AppendLine("Ngày nộp đơn: " + CommittingDate);
            sb.AppendLine("Ngày bắt đầu nghỉ: " + StartLeaveDate);
            sb.AppendLine("Ngày kết thúc nghỉ: " + EndLeaveDate);
            sb.AppendLine("Số ngày nghỉ: " + NumberOfLeaveDays);
            sb.AppendLine("Các ngày khác: " + string.Join(", ", UndefinedDates));
            sb.AppendLine("Tên người: " + string.Join(", ", EmployeeNames));
            return sb.ToString();
        }
    }
}
