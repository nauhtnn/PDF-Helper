using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib.Ner
{
    // Rule engine to manage and apply rules
    public class RuleEngine
    {
        private readonly List<Rule> rules = new List<Rule>();

        public void AddRule(Rule rule)
        {
            rules.Add(rule);
        }

        public void ApplyRules(IEnumerable<Token> tokens)
        {
            foreach (var token in tokens)
            {
                foreach (var rule in rules)
                {
                    rule.Apply(token);
                }
            }
        }
    }
}
