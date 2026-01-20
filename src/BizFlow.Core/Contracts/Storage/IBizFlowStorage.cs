using BizFlow.Core.Model;

namespace BizFlow.Core.Contracts.Storage
{
    public interface IBizFlowStorage : IDisposable
    {
        string Ping();

        Task AddPipelineAsync(Pipeline pipelineItem, CancellationToken cancellationToken = default);
    }
}


