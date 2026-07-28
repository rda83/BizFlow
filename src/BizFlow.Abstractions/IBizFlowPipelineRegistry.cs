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
        Task<IReadOnlyList<PipelineDefinition>> GetAllAsync(CancellationToken ct = default);
        Task CreateAsync(PipelineDefinition pipeline, CancellationToken ct = default);
        Task<bool> RemoveAsync(string pipelineName, CancellationToken ct = default);

        event EventHandler<PipelineDefinition>? PipelineAdded;
        event EventHandler<string>? PipelineRemoved;
    }
}
