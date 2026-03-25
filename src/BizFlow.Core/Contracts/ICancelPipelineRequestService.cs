using BizFlow.Core.Model;

namespace BizFlow.Core.Contracts
{
    public interface ICancelPipelineRequestService
    {
        Task<long> AddAsync(CancellationRequest request, CancellationToken cancellationToken = default);
        Task<CancellationRequest?> GetActiveRequest(string pipelineName, CancellationToken cancellationToken = default);
        Task SetExecutedAsync(long id, string msg = "", CancellationToken cancellationToken = default);        
    }
}
