using BizFlow.Abstractions.Model;

namespace BizFlow.Core.Internal.Features.CancelPipeline
{
    public interface ICancelPipelineHandler
    {
        Task<BizFlowCommandResult> CancelPipeline(CancelPipelineCommand command,
            CancellationToken cancellationToken = default);

        Task<BizFlowCommandResult> CloseCancellationRequest(CloseCancelPipelineCommand command,
            CancellationToken cancellationToken = default);
    }
}
