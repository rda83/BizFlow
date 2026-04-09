using BizFlow.Core.Model;

namespace BizFlow.Core.Contracts
{
    public interface ICancelPipelineRequestService
    {
        Task<CancellationRequest?> GetActiveRequest(string pipelineName, CancellationToken cancellationToken = default);
    }
}
