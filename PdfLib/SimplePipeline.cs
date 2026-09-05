using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PdfLib
{
    // Pipeline orchestrator
    public class SimplePipeline
    {
        private readonly List<PipelineStage> _stages = new List<PipelineStage>();

        public void AddStage(PipelineStage stage)
        {
            _stages.Add(stage);
        }

        public IReadOnlyList<PipelineStage> GetStages() => _stages.AsReadOnly();

        public BaseEntity Run(BaseEntity input)
        {
            BaseEntity currentOutput = input;
            foreach (var stage in _stages)
            {
                currentOutput = stage.Execute(currentOutput);
            }
            return currentOutput;
        }

    }
}