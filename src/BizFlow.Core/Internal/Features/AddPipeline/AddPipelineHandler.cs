using BizFlow.Core.Contracts;
using BizFlow.Core.Internal.Shared;
using BizFlow.Core.Model;

namespace BizFlow.Core.Internal.Features.AddPipeline
{
    internal class AddPipelineHandler : IAddPipelineHandler
    {
        private readonly BizFlowJobManager bizFlowJobManager;
        private readonly IPipelineService pipelineService;

        public AddPipelineHandler(BizFlowJobManager bizFlowJobManager,
            IPipelineService pipelineService)
        {
            this.bizFlowJobManager = bizFlowJobManager;
            this.pipelineService = pipelineService;
        }

        public Task<BizFlowChangingResult> AddPipelineAsync(AddPipelineCommand command)
        {
            throw new NotImplementedException();
        }
    }
}
