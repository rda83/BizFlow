using BizFlow.Core.Model;

namespace BizFlow.Core.Contracts
{
    public interface IPipelineService
    {
        IReadOnlyCollection<Pipeline> GetPipelines();
        Pipeline GetPipeline(string pipelineName);
        Task AddPipelineAsync(Pipeline pipelineItem, CancellationToken cancellationToken = default);
        Task<bool> PipelineNameExist(string pipelineName, CancellationToken cancellationToken = default);
    }
}
