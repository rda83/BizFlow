using BizFlow.Abstractions.Model;

namespace BizFlow.Core.Internal.Features.StartNowPipeline
{
    public interface IStartNowPipelineHandler
    {
        Task<BizFlowCommandResult> StartNowPipelineAsync(StartNowPipelineCommand command,
            CancellationToken cancellationToken = default);
    }
}
