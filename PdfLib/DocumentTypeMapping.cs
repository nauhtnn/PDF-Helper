using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OcrLib
{
    public static class DocumentTypeMapping
    {
        public static readonly Dictionary<DocumentType, string> UpperMarked =
        new Dictionary<DocumentType, string>
        {
            { DocumentType.LeaveSlip, "GIẤY NGHỈ PHÉP" },
            { DocumentType.LeaveRequest, "ĐƠN XIN NGHỈ PHEP" },
            { DocumentType.Decision, "QUYẾT ĐỊNH" },
            { DocumentType.Announcement, "THÔNG BÁO" },
            { DocumentType.Report, "BÁO CÁO" },
            { DocumentType.Invitation1, "THƯ MỜI" },
            { DocumentType.Invitation2, "GIẤY MỜI" },
            { DocumentType.MeetingDelay, "GIẤY DỜI HỌP" },
            { DocumentType.Minutes, "BIÊN BẢN" },
            { DocumentType.KKTS1, "KÊ KHAI TÀI SẢN" },
            { DocumentType.KKTS2, "BẢN KÊ KHAI TÀI SẢN" },
            { DocumentType.KKTS3, "PHIẾU KÊ KHAI TÀI SẢN" },
            { DocumentType.BSLL, "PHIẾU BỔ SUNG LÝ LỊCH" },
            { DocumentType.SYLL, "SƠ YẾU LÝ LỊCH" },
            { DocumentType.PHIEU_DIEU_TRA, "PHIẾU ĐIỀU TRA" },
            { DocumentType.LUAT, "LUẬT" },
            { DocumentType.NGHI_DINH, "NGHỊ ĐỊNH" },
            { DocumentType.THONG_TU, "THÔNG TƯ" }
        };

        public static readonly Dictionary<DocumentType, string> UpperUnmarked =
            UpperMarked.ToDictionary(
                kvp => kvp.Key,
                kvp => TextMeasurement.RemoveAccent(kvp.Value)
            );

        public static readonly Dictionary<DocumentType, string> SentenceCase =
            UpperMarked.ToDictionary(
                kvp => kvp.Key,
                kvp => TextMeasurement.SentenceCase(kvp.Value)
            );

        public static DocumentType ParseUpperDocumentTypeLine(string line)
        {
            var simpleLine = TextMeasurement.RemoveAccent(line);
            foreach (var mapping in DocumentTypeMapping.UpperUnmarked)
            {
                if (simpleLine.StartsWith(mapping.Value))
                {
                    return mapping.Key;
                }
            }
            return DocumentType.Unknown;
        }
    }
}
