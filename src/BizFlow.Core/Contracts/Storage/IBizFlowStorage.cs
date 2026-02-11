using BizFlow.Core.Model;

namespace BizFlow.Core.Contracts.Storage
{
    public interface IBizFlowStorage : IDisposable
    {
        Task AddPipelineAsync(Pipeline pipelineItem, CancellationToken cancellationToken = default);
        Task<bool> PipelineNameExistAsync(string pipelineName, CancellationToken cancellationToken = default);
    }
}


