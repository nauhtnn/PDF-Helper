using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace PdfLib
{
    public sealed partial class NerLeaveSlip : DocumentProcessor
    {
        public Dictionary<string, LeaveSlip> LeaveSlips;

        static NerLeaveSlip _instance;
        
        public static NerLeaveSlip Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new NerLeaveSlip();
                }
                return _instance;
            }
        }

        NerLeaveSlip()
        {
            LeaveSlips = new Dictionary<string, LeaveSlip>();
            FileTypes = new string[] { "*.txt" };
        }

        protected override void ProcessFileCore(string filePath)
        {
            if(LeaveSlips.ContainsKey(filePath))
            {
                StatusMessage.Instance.AddMessage($"File {filePath} đã được xử lý trước đó, bỏ qua.");
                return;
            }

            _NER_File(filePath);
        }

        bool IsCommittingDate(string sentence)
        {
            string lowerSentence = sentence.ToLower();
            if (lowerSentence.Contains("đơn "))
            {
                return true;
            }
            return false;
        }

        bool IsStartLeaveDate(string sentence)
        {
            string lowerSentence = sentence.ToLower();

            if (!lowerSentence.Contains("nghỉ")) {return false; }

            if(lowerSentence.Contains("từ ")) { return true; }

            if(lowerSentence.Contains("trong thời gian"))
            {
                return true;
            }

            return false;
        }

        int GetNumberOfLeaveDays(string sentence)
        {
            Match match = Regex.Match(sentence, @"\b\d+\s+ngày\b");
            if (match.Success)
            {
                string numberString = match.Value.Split(' ')[0];
                if (int.TryParse(numberString, out int numberOfDays))
                {
                    return numberOfDays;
                }
            }
            return 0;
        }

        void _NER_File(string filePath)
        {
            StatusMessage.Instance.AddMessage($"Bắt đầu đọc file: {filePath}.");

            var documentInfo = new DocumentGeneralInfo();
            documentInfo.ProcessFile(filePath);

            if(documentInfo.DetectedDocType != DocumentType.LeaveSlip &&
                documentInfo.DetectedDocType != DocumentType.LeaveRequest)
            {
                StatusMessage.Instance.AddMessage($"File {filePath} không phải là giấy nghỉ phép, bỏ qua.");
                return;
            }

            LeaveSlip leaveSlip = new LeaveSlip();

            leaveSlip.FilePath = filePath;

            foreach (string sentence in documentInfo.Sentences)
            {
                MatchCollection dates = Regex.Matches(sentence, @"\b\d{1,2}(/|-|\.)\d{1,2}(/|-|\.)\d{4}\b");

                if(dates.Count > 0)
                {
                    if (IsCommittingDate(sentence))
                    {
                        leaveSlip.CommittingDate = dates[0].Value;

                        if(dates.Count > 1)
                        {
                            for(int i = 1; i < dates.Count; i++)
                            {
                                leaveSlip.AddUndefinedDate(dates[i].Value);
                            }
                        }
                    }
                    else if (IsStartLeaveDate(sentence))
                    {
                        leaveSlip.StartLeaveDate = dates[0].Value;
                        if(dates.Count > 1)
                        {
                            leaveSlip.EndLeaveDate = dates[1].Value;
                        }
                        if(dates.Count > 2)
                        {
                            for (int i = 2; i < dates.Count; i++)
                            {
                                leaveSlip.AddUndefinedDate(dates[i].Value);
                            }
                        }

                        leaveSlip.NumberOfLeaveDays = GetNumberOfLeaveDays(sentence);
                    }
                    else
                    {
                        foreach (Match date in dates)
                        {
                            leaveSlip.AddUndefinedDate(date.Value);
                        }
                    }
                }

                List<string> employeeNames = PredefinedPayroll.GetInstance().GetOccurrences(sentence);
                if(employeeNames.Count > 0)
                {
                    leaveSlip.AddEmployeeNames(employeeNames);
                }
            }

            LeaveSlips.Add(filePath, leaveSlip);

            StatusMessage.Instance.AddMessage("Tìm thấy:" + leaveSlip.ToString());
        }
    }
}