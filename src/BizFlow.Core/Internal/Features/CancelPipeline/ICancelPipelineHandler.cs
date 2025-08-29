using BizFlow.Core.Model;

namespace BizFlow.Core.Internal.Features.CancelPipeline
{
    internal interface ICancelPipelineHandler
    {
        Task<BizFlowChangingResult> CancelPipeline(CancelPipelineCommand command,
            CancellationToken cancellationToken = default);
    }
}
