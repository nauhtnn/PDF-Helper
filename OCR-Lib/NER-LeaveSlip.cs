using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace OCR_Lib
{
    public sealed class NER_LeaveSlip : DocumentProcessor
    {
        public Dictionary<string, LeaveSlip> LeaveSlips;

        public static NER_LeaveSlip instance;
        
        public static NER_LeaveSlip GetInstance()
        {
            if (instance == null)
            {
                instance = new NER_LeaveSlip();
            }
            return instance;
        }

        NER_LeaveSlip()
        {
            LeaveSlips = new Dictionary<string, LeaveSlip>();
            FileTypes = new string[] { "*.txt" };
        }

        protected override void ProcessFileCore(string filePath)
        {
            if (!File.Exists(filePath))
            {
                StatusMessage.GetInstance().AddMessage(filePath + " không phải là đường dẫn của một file !");
                return;
            }

            if(LeaveSlips.ContainsKey(filePath))
            {
                StatusMessage.GetInstance().AddMessage($"File {filePath} đã được xử lý trước đó, bỏ qua.");
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
            StatusMessage.GetInstance().AddMessage($"Bắt đầu đọc file: {filePath}.");

            string[] lines = File.ReadAllLines(filePath);

            List<string> wrappedLines = BuildWrapLines(lines);

            List<string> sentences = BuildSentences(wrappedLines);

            LeaveSlip leaveSlip = new LeaveSlip();

            leaveSlip.FilePath = filePath;

            foreach (string sentence in sentences)
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

            StatusMessage.GetInstance().AddMessage("Tìm thấy:" + leaveSlip.ToString());
        }

        List<string> BuildSentences(List<string> wrappedLines)
        {
            List<string> sentences = new List<string>();
            foreach (string line in wrappedLines)
            {
                // Simple sentence splitting logic - replace with actual NER sentence splitting as needed
                string[] subSentences = line.Split(new char[] { '.', '!', '?' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string subSentence in subSentences)
                {
                    string trimmedSentence = subSentence.Trim();
                    if (!string.IsNullOrEmpty(trimmedSentence))
                    {
                        sentences.Add(trimmedSentence);
                    }
                }
            }
            return sentences;
        }

        List<string> BuildWrapLines(string[] lines)
        {
            List<string> wrappedLines = new List<string>();
            System.Text.StringBuilder joinedLine = new System.Text.StringBuilder();
            int newLineCount = 0;
            foreach (string line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (++newLineCount >= 2)
                    {
                        wrappedLines.Add(joinedLine.ToString().Trim());
                        joinedLine.Clear();
                        newLineCount = 0;
                    }
                }
                else
                {
                    joinedLine.Append(line + " ");
                }
            }

            // Add the last joined line if it has content
            if (joinedLine.Length > 0)
            {
                wrappedLines.Add(joinedLine.ToString().Trim());
            }

            return wrappedLines;
        }
    }
}