using BizFlow.Core.Contracts.Storage;
using BizFlow.Core.Internal.Shared;
using BizFlow.Core.Model;
namespace BizFlow.Core.Internal.Features.StartNowPipeline
{
    public class StartNowPipelineHandler : IStartNowPipelineHandler
    {
        private readonly BizFlowJobManager _bizFlowJobManager;
        private readonly IBizFlowStorage _storage;

        public StartNowPipelineHandler(BizFlowJobManager bizFlowJobManager, IBizFlowStorage storage)
        {
            _bizFlowJobManager = bizFlowJobManager;
            _storage = storage;
        }

        public async Task<BizFlowChangingResult> StartNowPipelineAsync(StartNowPipelineCommand command,
            CancellationToken cancellationToken = default)
        {
            var pipelineName = command.Name;

            var result = new BizFlowChangingResult() { Success = true };
            try
            {
                var isPipelineNameExist = await _storage.PipelineNameExistAsync(pipelineName, cancellationToken);
                if (!isPipelineNameExist)
                {
                    result.Success = false;
                    result.Message = $"A pipeline with the name: {pipelineName} does not exist.";
                    return result;
                }

                var launchId = Guid.NewGuid().ToString();
                await _bizFlowJobManager.StartNow(pipelineName, launchId, cancellationToken);

                result.Message = $"Pipeline: {pipelineName} has been started.";
                result.LaunchId = launchId;

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Error starting pipeline {pipelineName}: {ex.Message}";
                return result;
            }          
        }
    }
}
