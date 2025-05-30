using BizFlow.Core.Model;

namespace BizFlow.Core.Contracts
{
    public interface IPipelineService
    {
        Task<IReadOnlyCollection<Pipeline>> GetPipelinesAsync(CancellationToken cancellationToken = default);
        Task<Pipeline?> GetPipelineAsync(string pipelineName, CancellationToken cancellationToken = default);
        Task AddPipelineAsync(Pipeline pipelineItem, CancellationToken cancellationToken = default);
        Task<bool> PipelineNameExist(string pipelineName, CancellationToken cancellationToken = default);
    }
}
