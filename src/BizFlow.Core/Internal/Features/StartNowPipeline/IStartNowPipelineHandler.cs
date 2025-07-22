using BizFlow.Core.Model;

namespace BizFlow.Core.Internal.Features.StartNowPipeline
{
    public interface IStartNowPipelineHandler
    {
        Task<BizFlowChangingResult> StartNowPipelineAsync(StartNowPipelineCommand command,
            CancellationToken cancellationToken = default);
    }
}
