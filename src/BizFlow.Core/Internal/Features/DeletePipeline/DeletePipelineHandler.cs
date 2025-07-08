using BizFlow.Core.Contracts;
using BizFlow.Core.Internal.Shared;
using BizFlow.Core.Model;

namespace BizFlow.Core.Internal.Features.DeletePipeline
{
    public class DeletePipelineHandler : IDeletePipelineHandler
    {
        private readonly BizFlowJobManager _bizFlowJobManager;
        private readonly IPipelineService _pipelineService;

        public DeletePipelineHandler(BizFlowJobManager bizFlowJobManager,
            IPipelineService pipelineService)
        {
            _bizFlowJobManager = bizFlowJobManager;
            _pipelineService = pipelineService;
        }

        public async Task<BizFlowChangingResult> DeletePipelineAsync(DeletePipelineCommand command, 
            CancellationToken cancellationToken = default)
        {
            var result = new BizFlowChangingResult() { Success = true };

            try
            {
                var pipelineNameExist = await _pipelineService.PipelineNameExist(command.Name, cancellationToken);
                if (!pipelineNameExist)
                {
                    result.Success = false;
                    result.Message = $"Пайплайна с именем: {command.Name} не существует."; // TODO:i18n
                }
                else
                {
                    await _pipelineService.DeletePipelineAsync(command.Name, cancellationToken);
                    await _bizFlowJobManager.DeleteTrigger(command.Name, cancellationToken);
                }
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.Message = $"{ex.Message}";
            }

            return result;
        }
    }
}
