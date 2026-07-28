using BizFlow.Abstractions;
using BizFlow.Abstractions.Model;

namespace BizFlow.Core
{
    public class DefaultBizFlowPipelineRegistry : IBizFlowPipelineRegistry
    {
        //private readonly ConcurrentDictionary<string, PipelineDefinition> _jobDefinitions = new();


        public event EventHandler<PipelineDefinition>? PipelineAdded;
        public event EventHandler<string>? PipelineRemoved;

        public Task<IReadOnlyList<PipelineDefinition>> GetAllAsync(CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task CreateAsync(PipelineDefinition pipeline, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }

        public Task<bool> RemoveAsync(string pipelineName, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
