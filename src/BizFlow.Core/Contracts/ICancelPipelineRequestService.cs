using BizFlow.Core.Model;

namespace BizFlow.Core.Contracts
{
    public interface ICancelPipelineRequestService
    {
        Task<long> AddAsync(CancelPipelineRequest request, CancellationToken cancellationToken = default);
    }
}
