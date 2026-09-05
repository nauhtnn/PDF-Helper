using PdfLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    // Base class for all stages
    public abstract class PipelineStage
    {
        public string Name { get; }
        public string Description { get; }

        protected PipelineStage(string name, string description)
        {
            Name = name;
            Description = description;
        }

        // Each stage must implement its own Execute logic
        public abstract BaseEntity Execute(BaseEntity input);
    }
}
