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
        public DocumentList LeaveSlips;

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
            LeaveSlips = new DocumentList();
            FileTypes = new string[] { "*.txt" };
        }

        protected override void ProcessFileCore(string filePath)
        {
            _NER_File(filePath);
        }

        void _NER_File(string filePath)
        {
            StatusMessage.Instance.AddMessage($"Bắt đầu các bước nhận dạng file: {filePath}.");

            var moreLeaveSlips = Cleaning(filePath);

            SplitSentence(moreLeaveSlips, filePath);

            moreLeaveSlips = ParseLeaveSlip(moreLeaveSlips, filePath);
            LeaveSlips.Documents.AddRange(moreLeaveSlips.Documents);
        }

        void WriteStageDebug(string saveFilePath, DocumentList documents)
        {
            if (!OcrSettings.GetInstance().IsGaussianBlurEnabled)
                return;
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
            string saveBaseName = Path.GetFileNameWithoutExtension(filePath) + "_NER_1cleaned.temp";
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
            string saveBaseName = Path.GetFileNameWithoutExtension(filePath) + "_NER_2sentences.temp";
            string saveFilePath = PathHelper.Instance.GenerateFile(saveBaseName, saveDirectory);

            WriteStageDebug(saveFilePath, documents);

            StatusMessage.Instance.AddMessage("Hoàn thành phân đoạn các câu.");
        }

        DocumentList ParseLeaveSlip(DocumentList documents, string filePath)
        {
            StatusMessage.Instance.AddMessage("Bắt đầu đọc giấy nghỉ phép.");

            LeaveSlipRuleStage leaveSlipRuleStage = new LeaveSlipRuleStage();

            leaveSlipRuleStage.ResetDefaults();
            DocumentList leaveSlips = leaveSlipRuleStage.Execute(documents) as DocumentList;

            if(OcrSettings.GetInstance().IsGaussianBlurEnabled)
            {
                string saveDirectory = Path.GetDirectoryName(filePath);
                string saveBaseName = Path.GetFileNameWithoutExtension(filePath) + "_NER_3GNP.temp";
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

                StatusMessage.Instance.AddMessage($"Đã xuất danh sách giấy nghỉ phép ra file: {saveFilePath}.");
            }

            StatusMessage.Instance.AddMessage($"Hoàn thành các bước nhận dạng file: {filePath}.");

            return leaveSlips;
        }
    }
}