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

        void ParseRegNumber(LeaveSlip slip)
        {
            foreach (string sentence in slip.TextBlock)
            {

                Match match = Regex.Match(sentence, @"S[ốó](.{2,10}GNP-TNI)");
                if (match.Success)
                {
                    slip.RegNumber = Regex.Replace(match.Groups[1].Value, @"\s+", "").Replace(":", "");
                    return;
                }

                match = Regex.Match(sentence, @"^Số\s*:\s*(\d+.{5,10}-.{1,5})");
                if (match.Success)
                {
                    slip.RegNumber = Regex.Replace(match.Groups[1].Value, @"\s+", "");
                    return;
                }

                match = Regex.Match(sentence, @"S[ốó](.{5,15}Q[ĐD]-TNI)");
                if (match.Success)
                {
                    slip.RegNumber = Regex.Replace(match.Groups[1].Value, @"\s+", "").Replace(":", "");
                    return;
                }
            }
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
                    string name = match.Groups["name"].Value.Trim();
                    if (!Regex.IsMatch(name, "\\b(" + CommonRegexPattern.CorePersonNamePrefixPattern + ")\\b"))
                    {
                        candidates.Add(name);
                        continue;
                    }
                }
            }

            string longestName = candidates.OrderByDescending(name => name.Length).FirstOrDefault();
            if (!string.IsNullOrEmpty(longestName))
            {
                slip.BossName = longestName;
            }
        }

        void ParsePublishedDate(LeaveSlip slip)
        {
            foreach (string sentence in slip.TextBlock)
            {
                Match match = Regex.Match(sentence, @"ngày(?<day>.{1,6})tháng(?<month>.{1,6})năm(?<year>.{1,6})");
                if (match.Success)
                {
                    slip.PublishedDate = Regex.Replace($"{match.Groups["day"].Value}/{match.Groups["month"].Value}/{match.Groups["year"].Value}",
                        @"\s+", "");
                    return;
                }
            }
        }

        // This method is for documents that are not recognized as leave slips
        string ParseAllDatesNotPublishedDate(LeaveSlip slip)
        {
            List<string> dates = new List<string>();
            foreach (string sentence in slip.TextBlock)
            {
                MatchCollection matches =
                    Regex.Matches(sentence, @"(?<day>\d.{1,3})/(?<month>.{1,4})/(?<year>.{1,5}\d)");
                foreach (Match match in matches)
                {
                    dates.Add(Regex.Replace($"{match.Groups["day"].Value.Trim()}/{match.Groups["month"].Value.Trim()}/{match.Groups["year"].Value.Trim()}",
                        @"\s+", ""));
                }
            }

            return string.Join(", ", dates);
        }

        // This method is for documents that are not recognized as leave slips
        string ParseAllNames(LeaveSlip slip)
        {
            List<string> candidates = new List<string>();
            foreach (string sentence in slip.TextBlock)
            {
                List<string> names = GetAllMatches(sentence, @"của\s+" +
                    CommonRegexPattern.CorePersonNamePrefixPattern +
                    @"\s+(?<name>(" + CommonRegexPattern.PascalCasePattern + "))",
                    "name");

                candidates.AddRange(names);

                names = GetAllMatches(sentence,
                    CommonRegexPattern.LaxPersonNamePrefixPattern +
                    @"\s*(?<name>(" + CommonRegexPattern.PascalCasePattern +
                    @"))$", "name");

                candidates.AddRange(names);

                names = GetAllMatches(sentence,
                    CommonRegexPattern.LaxPersonNamePrefixPattern +
                    @"\s*(?<name>(" + CommonRegexPattern.PascalCasePattern +
                    @"))\s*[;:]",
                    "name");

                candidates.AddRange(names);

                names = GetAllMatches(sentence,
                    CommonRegexPattern.StrictPersonNamePrefixPattern +
                    @"\s*(?<name>(" + CommonRegexPattern.PascalCasePattern +
                    @"|" + CommonRegexPattern.UppercaseWordsPattern + @"))",
                    "name");

                candidates.AddRange(names);
            }

            return string.Join(", ", candidates);
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

        List<string> GetAllMatches(string sentence, string pattern, string groupName)
        {
            MatchCollection matches = Regex.Matches(sentence, pattern);
            List<string> results = new List<string>();
            foreach (Match match in matches)
            {
                results.Add(match.Groups[groupName].Value.Trim());
            }
            return results;
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

                ParseRegNumber(leaveSlip);

                ParsePublishedDate(leaveSlip);

                if (leaveSlip.DocTypes.Count == 1 &&
                    leaveSlip.DocTypes[0] == DocumentType.LeaveSlip)
                {
                    foreach (var rule in Rules)
                    {
                        rule.Apply(leaveSlip);
                    }

                    ParseEmployeeName(leaveSlip);

                    ParseBossName(leaveSlip);
                }
                else
                {
                    leaveSlip.UndefinedDates = new List<string>();
                    leaveSlip.UndefinedDates.Add(ParseAllDatesNotPublishedDate(leaveSlip));
                    leaveSlip.CommittingDate = string.Join(", ", leaveSlip.UndefinedDates);
                    leaveSlip.EmployeeName = ParseAllNames(leaveSlip);
                }

                leaveSlips.Documents.Add(leaveSlip);
            }

            return leaveSlips;
        }
    }
}
