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

        public void ResetDefaults()
        {
            Rules.Clear();

            Rule regNumber = new Rule(
                "RegNumber",
                (entity) => {
                    LeaveSlip slip = entity as LeaveSlip;
                    if(slip == null)
                        return false;

                    foreach(string sentence in slip.TextBlock)
                    {
                            string pattern = @"Số:\s*(\d+\s*[^A-Za-z0-9\s]*\s*[\p{Lu}]+\s*-\s*[\p{Lu}]+)";

                            Match match = Regex.Match(sentence, pattern);
                            if (match.Success)
                            {
                                slip.RegNumber = Regex.Replace(match.Groups[1].Value, @"\s+", "");
                                return true;
                            }
                    }
                    return false;
                },
                (entity) => {}
            );

            Rules.Add(regNumber);
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
