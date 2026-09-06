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
        public Func<BaseEntity, bool> Condition { get; set; }
        public Action<BaseEntity> Action { get; set; }

        public Rule(string name, Func<BaseEntity, bool> condition, Action<BaseEntity> action)
        {
            Name = name;
            Condition = condition;
            Action = action;
        }

        public void Apply(BaseEntity entity)
        {
            if (Condition(entity))
            {
                Action(entity);
            }
        }
    }
}
