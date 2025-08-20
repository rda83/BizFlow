using BizFlow.Core.Model;

namespace BizFlow.Core.Internal.Features.StatusPipeline
{
    public interface IStatusPipelineHandler
    {
        Task<StatusPipelineResult> StatusPipeline(StatusPipelineCommand command,
            CancellationToken cancellationToken = default);
    }
}
