using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PdfLib
{
    public class LeaveSlipRuleStage : PipelineStage
    {
        public List<Rule> Rules = new List<Rule>();

        public LeaveSlipRuleStage()
            : base("LeaveSlipRuleStage", "Applies rules for extracting information from leave slip documents") { }

        public void AddRule(Rule rule)
        {
            Rules.Add(rule);
        }

        private Rule CreateRule(string name, string pattern, Func<Match, LeaveSlip, bool> onMatch)
        {
            return new Rule(
                name,
                entity =>
                {
                    var slip = entity as LeaveSlip;
                    if (slip == null)
                        return false;

                    foreach (string sentence in slip.TextBlock)
                    {
                        Match match = Regex.Match(sentence, pattern);
                        if (match.Success)
                        {
                            return onMatch(match, slip);
                        }
                    }
                    return false;
                },
                entity => { }
            );
        }

        public void ResetDefaults()
        {
            Rules.Clear();

            Rules.Add(CreateRule(
                "RegNumber",
                @"Số:\s*(\d+\s*[^A-Za-z0-9\s]*\s*[\p{Lu}]+\s*-\s*[\p{Lu}]+)",
                (match, slip) =>
                {
                    slip.RegNumber = Regex.Replace(match.Groups[1].Value, @"\s+", "");
                    return true;
                }
            ));

            Rules.Add(CreateRule(
                "PublishedDate",
                @"ngày\s*(?<day>\d+)\s*tháng\s*(?<month>\d+)\s*năm\s*(?<year>\d+)",
                (match, slip) =>
                {
                    slip.PublishedDate = $"{match.Groups["day"].Value}/{match.Groups["month"].Value}/{match.Groups["year"].Value}";
                    return true;
                }
            ));

            Rules.Add(CreateRule(
                "CommittingDate",
                @"ngày\s*(\d+/\d+/\d+)\s*của",
                (match, slip) =>
                {
                    slip.CommittingDate = match.Groups[1].Value;
                    return true;
                }
            ));

            Rules.Add(CreateRule(
                "FromDate",
                @"[Tt]ừ\s*ngày\s*(\d+/\d+/\d+)\s",
                (match, slip) =>
                {
                    slip.StartLeaveDate = match.Groups[1].Value;
                    return true;
                }
            ));

            Rules.Add(CreateRule(
                "ToDate",
                @"đến\s*ngày\s*(\d+/\d+/\d+)\s",
                (match, slip) =>
                {
                    slip.EndLeaveDate = match.Groups[1].Value;
                    return true;
                }
            ));

            Rules.Add(CreateRule(
                "NumberOfLeaveDays",
                @"(\d+)\s*ngày",
                (match, slip) =>
                {
                    slip.NumberOfLeaveDays = int.Parse(match.Groups[1].Value);
                    return true;
                }
            ));
        }

        void ParseBossName(LeaveSlip slip)
        {
            List<string> candidates = new List<string>();

            foreach (string sentence in slip.TextBlock)
            {
                if (TextMeasurement.FuzzyStartsWith(sentence, "Nơi nhận:"))
                {
                    candidates.Clear();
                    continue;
                }
                    
                Match match = Regex.Match(sentence, @"^(?<name>" +
                    CommonRegexPattern.PascalCasePattern + ")$");
                if (match.Success)
                {
                    candidates.Add(match.Groups["name"].Value.Trim());
                    continue;
                }
            }

            string longestName = candidates.OrderByDescending(name => name.Length).FirstOrDefault();
            if (!string.IsNullOrEmpty(longestName))
            {
                slip.BossName = longestName;
            }
        }

        void ParseEmployeeName(LeaveSlip slip)
        {
            List<string> candidates = new List<string>();
            foreach(string sentence in slip.TextBlock)
            {
                Match match = Regex.Match(sentence, @"của\s+" +
                    CommonRegexPattern.CorePersonNamePrefixPattern +
                    @"\s+(?<name>(" + CommonRegexPattern.PascalCasePattern + "))");
                if (match.Success)
                {
                    candidates.Add(match.Groups["name"].Value.Trim());
                    continue;
                }

                match = Regex.Match(sentence,
                    CommonRegexPattern.LaxPersonNamePrefixPattern +
                    @"\s*(?<name>(" + CommonRegexPattern.PascalCasePattern +
                    @"))$");

                if (match.Success)
                {
                    candidates.Add(match.Groups["name"].Value.Trim());
                }

                match = Regex.Match(sentence,
                    CommonRegexPattern.LaxPersonNamePrefixPattern +
                    @"\s*(?<name>(" + CommonRegexPattern.PascalCasePattern +
                    @"))\s*[;:]");

                if (match.Success)
                {
                    candidates.Add(match.Groups["name"].Value.Trim());
                }

                match = Regex.Match(sentence,
                    CommonRegexPattern.StrictPersonNamePrefixPattern +
                    @"\s*(?<name>(" + CommonRegexPattern.PascalCasePattern +
                    @"|" + CommonRegexPattern.UppercaseWordsPattern + @"))");

                if (match.Success)
                {
                    candidates.Add(match.Groups["name"].Value.Trim());
                }
            }

            if(candidates.Count > 0)
            {
                List<string> noDuplicated = new List<string>();
                foreach (string name in candidates)
                {
                    if (!noDuplicated.Contains(name))
                    {
                        noDuplicated.Add(name);
                    }
                }
                
                slip.EmployeeName = string.Join(", ", noDuplicated);
            }
        }

        public override BaseEntity Execute(BaseEntity input)
        {
            DocumentList documents = input as DocumentList;

            if (documents == null)
            {
                throw new ArgumentException("Input must be of type DocumentList", nameof(input));
            }

            DocumentList leaveSlips = new DocumentList();

            foreach (var document in documents.Documents)
            {
                LeaveSlip leaveSlip = new LeaveSlip(document);

                if(leaveSlip.DocTypes.Count == 1 &&
                    leaveSlip.DocTypes[0] == DocumentType.LeaveSlip)
                {
                    foreach (var rule in Rules)
                    {
                        rule.Apply(leaveSlip);
                    }
                }

                ParseEmployeeName(leaveSlip);

                ParseBossName(leaveSlip);

                leaveSlips.Documents.Add(leaveSlip);
            }

            return leaveSlips;
        }
    }
}
