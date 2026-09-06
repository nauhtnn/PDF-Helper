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

            Rules.Add(CreateRule(
                "EmployeeName",
                @"của\s*([Ôô]ng|[Bb]à)\s*(?<name>(?:[A-ZÀ-Ỵ][a-zà-ỵ]+(?:\s+[A-ZÀ-Ỵ][a-zà-ỵ]+)*))",
                (match, slip) =>
                {
                    slip.EmployeeName = match.Groups["name"].Value.Trim();
                    return true;
                }
            ));

            Rules.Add(CreateRule(
                "BossName",
               @"^(?<name>(?:[A-ZÀ-Ỵ][a-zà-ỵ]+(?:\s+[A-ZÀ-Ỵ][a-zà-ỵ]+)*))$",
                (match, slip) =>
                {
                    slip.BossName = match.Groups["name"].Value.Trim();
                    return true;
                }
            ));
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
                foreach (var rule in Rules)
                {
                    rule.Apply(leaveSlip);
                }
                leaveSlips.Documents.Add(leaveSlip);
            }

            return leaveSlips;
        }
    }
}
