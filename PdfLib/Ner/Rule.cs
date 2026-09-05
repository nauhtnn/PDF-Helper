using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    // Simple rule definition
    public class Rule
    {
        public string Name { get; set; }
        public Func<Token, bool> Condition { get; set; }
        public Action<Token> Action { get; set; }

        public Rule(string name, Func<Token, bool> condition, Action<Token> action)
        {
            Name = name;
            Condition = condition;
            Action = action;
        }

        public void Apply(Token token)
        {
            if (Condition(token))
            {
                Action(token);
            }
        }
    }
}
