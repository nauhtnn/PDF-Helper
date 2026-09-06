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
            /*if(LeaveSlips.ContainsKey(filePath))
            {
                StatusMessage.Instance.AddMessage($"File {filePath} đã được xử lý trước đó, bỏ qua.");
                return;
            }*/

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

            /*var documentInfo = new DocumentGeneralInfo();
            documentInfo.ProcessFile(filePath);

            if(documentInfo.DetectedDocType != DocumentType.LeaveSlip &&
                documentInfo.DetectedDocType != DocumentType.LeaveRequest)
            {
                StatusMessage.Instance.AddMessage($"File {filePath} không phải là giấy nghỉ phép, bỏ qua.");
                return;
            }

            LeaveSlip leaveSlip = new LeaveSlip();

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
                                leaveSlip.UndefinedDates.Add(dates[i].Value);
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
                                leaveSlip.UndefinedDates.Add(dates[i].Value);
                            }
                        }

                        leaveSlip.NumberOfLeaveDays = GetNumberOfLeaveDays(sentence);
                    }
                    else
                    {
                        foreach (Match date in dates)
                        {
                            leaveSlip.UndefinedDates.Add(date.Value);
                        }
                    }
                }

                List<string> employeeNames = PredefinedPayroll.GetInstance().GetOccurrences(sentence);
                if(employeeNames.Count > 0)
                {
                    leaveSlip.EmployeeName = employeeNames[0];
                }
            }

            LeaveSlips.Add(filePath, leaveSlip);*/

            DocumentList documents = Cleaning(filePath);

            SplitSentence(documents, filePath);

            ParseLeaveSlip(documents, filePath);
        }

        void WriteStageDebug(string saveFilePath, DocumentList documents)
        {
            FileStream fileStream = File.OpenWrite(saveFilePath);

            int docIndex = 0;

            foreach (var d in documents.Documents)
            {
                docIndex++;
                byte[] textInBytes = Encoding.UTF8.GetBytes("\n<DOCUMENT " + docIndex + " />\n");
                fileStream.Write(textInBytes, 0, textInBytes.Length);

                foreach (var p in d.TextBlock)
                {
                    textInBytes = Encoding.UTF8.GetBytes(p + "\n");
                    fileStream.Write(textInBytes, 0, textInBytes.Length);
                }
            }

            fileStream.Flush();
            fileStream.Close();
        }

        DocumentList Cleaning(string filePath)
        {
            StatusMessage.Instance.AddMessage("Bắt đầu làm sạch dữ liệu.");

            LineFragment fragment = new LineFragment(File.ReadAllLines(filePath));

            TextCleaningStage stage = new TextCleaningStage();

            DocumentList documents = stage.Execute(fragment) as DocumentList;

            string saveDirectory = Path.GetDirectoryName(filePath);
            string saveBaseName = Path.GetFileNameWithoutExtension(filePath) + "-cleaned.txt";
            string saveFilePath = PathHelper.Instance.GenerateFile(saveBaseName, saveDirectory);

            WriteStageDebug(saveFilePath, documents);

            StatusMessage.Instance.AddMessage("Hoàn thành làm sạch dữ liệu.");

            return documents;
        }

        void SplitSentence(DocumentList documents, string filePath)
        {
            StatusMessage.Instance.AddMessage("Bắt đầu phân đoạn các câu.");

            SentenceSplittingStage stage = new SentenceSplittingStage();
            stage.Execute(documents);

            string saveDirectory = Path.GetDirectoryName(filePath);
            string saveBaseName = Path.GetFileNameWithoutExtension(filePath) + "-sentences.txt";
            string saveFilePath = PathHelper.Instance.GenerateFile(saveBaseName, saveDirectory);

            WriteStageDebug(saveFilePath, documents);

            StatusMessage.Instance.AddMessage("Hoàn thành phân đoạn các câu.");
        }

        void ParseLeaveSlip(DocumentList documents, string filePath)
        {
            StatusMessage.Instance.AddMessage("Bắt đầu đọc giấy nghỉ phép.");

            LeaveSlipRuleStage leaveSlipRuleStage = new LeaveSlipRuleStage();

            leaveSlipRuleStage.ResetDefaults();
            DocumentList leaveSlips = leaveSlipRuleStage.Execute(documents) as DocumentList;

            string saveDirectory = Path.GetDirectoryName(filePath);
            string saveBaseName = Path.GetFileNameWithoutExtension(filePath) + "-GNP.txt";
            string saveFilePath = PathHelper.Instance.GenerateFile(saveBaseName, saveDirectory);

            FileStream fileStream = File.OpenWrite(saveFilePath);

            foreach (var d in leaveSlips.Documents)
            {
                LeaveSlip leaveSlip = d as LeaveSlip;
                string p = leaveSlip.ToString();
                byte[] textInBytes = Encoding.UTF8.GetBytes(p + "\n");
                fileStream.Write(textInBytes, 0, textInBytes.Length);
            }

            fileStream.Flush();
            fileStream.Close();

            StatusMessage.Instance.AddMessage("Hoàn thành dò tìm giấy nghỉ phép. Đã xuất kết quả ra: " + saveFilePath);
        }
    }
}