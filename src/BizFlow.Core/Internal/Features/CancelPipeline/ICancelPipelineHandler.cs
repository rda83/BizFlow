using BizFlow.Core.Model;

namespace BizFlow.Core.Internal.Features.CancelPipeline
{
    public interface ICancelPipelineHandler
    {
        Task<BizFlowChangingResult> CancelPipeline(CancelPipelineCommand command,
            CancellationToken cancellationToken = default);
    }
}
