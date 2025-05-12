using BizFlow.Core.Model;

namespace BizFlow.Core.Internal.Features.AddPipeline
{
    public interface IAddPipelineHandler
    {
        Task<BizFlowChangingResult> AddPipelineAsync(AddPipelineCommand command, 
            CancellationToken cancellationToken = default);
    }
}
