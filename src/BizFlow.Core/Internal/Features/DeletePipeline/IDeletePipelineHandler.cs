using BizFlow.Abstractions.Model;

namespace BizFlow.Core.Internal.Features.DeletePipeline
{
    public interface IDeletePipelineHandler
    {
        Task<BizFlowCommandResult> DeletePipelineAsync(DeletePipelineCommand command,
            CancellationToken cancellationToken = default);
    }
}
