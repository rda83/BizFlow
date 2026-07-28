using BizFlow.Abstractions.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BizFlow.Abstractions
{
    public interface IBizFlowPipelineRegistry
    {
        IReadOnlyList<PipelineDefinition> GetAll();
        void Create(PipelineDefinition pipeline);
        bool Remove(string pipelineName);
        bool Exist(string pipelineName);

        event EventHandler<PipelineDefinition>? OnAdded;
        event EventHandler<string>? OnRemoved;
    }
}
