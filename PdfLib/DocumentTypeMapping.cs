using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    public static class DocumentTypeMapping
    {
        public static readonly Dictionary<DocumentType, string> UpperMarked =
        new Dictionary<DocumentType, string>
        {
            { DocumentType.LeaveSlip, "GIẤY NGHỈ PHÉP" },
            { DocumentType.LeaveRequest, "ĐƠN XIN NGHỈ PHÉP" },
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
            simpleLine = simpleLine.Replace("GLAY ", "GIAY ");
            simpleLine = simpleLine.Replace("NGHII ", "NGHI ");
            foreach (var mapping in DocumentTypeMapping.UpperUnmarked)
            {
                if (simpleLine.StartsWith(mapping.Value))
                {
                    return mapping.Key;
                }
            }
            return DocumentType.Unknown;
        }

        public static int LevenshteinDistance(string s, string t)
        {
            if (string.IsNullOrEmpty(s))
                return string.IsNullOrEmpty(t) ? 0 : t.Length;
            if (string.IsNullOrEmpty(t))
                return s.Length;
            int[,] d = new int[s.Length + 1, t.Length + 1];
            for (int i = 0; i <= s.Length; i++)
                d[i, 0] = i;
            for (int j = 0; j <= t.Length; j++)
                d[0, j] = j;
            for (int i = 1; i <= s.Length; i++)
            {
                for (int j = 1; j <= t.Length; j++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }
            return d[s.Length, t.Length];
        }

        public static DocumentType FuzzyParseDocumentTypeLine(string line)
        {
            int LevenshteinThreshold = 2; // Adjust this threshold as needed

            List<DocumentType> candidates = new List<DocumentType>();
            List<int> distances = new List<int>();

            var simpleTokens = TextMeasurement.Tokenize(TextMeasurement.RemoveAccent(line));
            foreach (var mapping in DocumentTypeMapping.SentenceCase)
            {
                var mappingTokens = TextMeasurement.Tokenize(TextMeasurement.RemoveAccent(mapping.Value));

                if(mappingTokens.Count == 1 || mappingTokens.Count > simpleTokens.Count)
                {
                    continue; // Skip if mapping has more tokens than the input line
                }

                int distance = 0;

                for (int i = 0; i < Math.Min(simpleTokens.Count, mappingTokens.Count); i++)
                {
                    distance += LevenshteinDistance(simpleTokens[i], mappingTokens[i]);
                }

                if (distance <= LevenshteinThreshold)
                {
                    candidates.Add(mapping.Key);
                    distances.Add(distance);
                }
            }

            if(candidates.Count > 0)
            {
                int minDistance = distances.Min();
                int index = distances.IndexOf(minDistance);
                return candidates[index];
            }

            return DocumentType.Unknown;
        }
    }
}
