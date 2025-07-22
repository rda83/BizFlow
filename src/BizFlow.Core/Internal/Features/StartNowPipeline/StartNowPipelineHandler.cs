using BizFlow.Core.Contracts;
using BizFlow.Core.Internal.Shared;
using BizFlow.Core.Model;
namespace BizFlow.Core.Internal.Features.StartNowPipeline
{
    public class StartNowPipelineHandler : IStartNowPipelineHandler
    {
        private readonly BizFlowJobManager _bizFlowJobManager;
        private readonly IPipelineService _pipelineService;

        public StartNowPipelineHandler(BizFlowJobManager bizFlowJobManager,
            IPipelineService pipelineService)
        {
            _bizFlowJobManager = bizFlowJobManager;
            _pipelineService = pipelineService;
        }

        public async Task<BizFlowChangingResult> StartNowPipelineAsync(StartNowPipelineCommand command,
            CancellationToken cancellationToken = default)
        {
            var pipelineName = command.Name;

            var result = new BizFlowChangingResult() { Success = true };
            try
            {
                var isPipelineNameExist = await _pipelineService.PipelineNameExist(pipelineName, cancellationToken);
                if (!isPipelineNameExist)
                {
                    result.Success = false;
                    result.Message = $"Пайплайна с именем: {pipelineName} не существует."; // TODO:i18n
                    return result;
                }

                var launchId = Guid.NewGuid().ToString();
                await _bizFlowJobManager.StartNow(pipelineName, launchId, cancellationToken);

                result.Message = $"Пайплайн: {pipelineName} запущен."; // TODO:i18n
                result.LaunchId = launchId;

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"Ошибка при запуске пайплайна {pipelineName}: {ex.Message}"; // TODO:i18n
                return result;
            }          
        }
    }
}
