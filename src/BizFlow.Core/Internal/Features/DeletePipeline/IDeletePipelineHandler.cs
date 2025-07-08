using BizFlow.Core.Model;

namespace BizFlow.Core.Internal.Features.DeletePipeline
{
    public interface IDeletePipelineHandler
    {
        Task<BizFlowChangingResult> DeletePipelineAsync(DeletePipelineCommand command,
            CancellationToken cancellationToken = default);
    }
}
