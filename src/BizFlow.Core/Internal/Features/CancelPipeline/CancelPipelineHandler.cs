using BizFlow.Core.Contracts;
using BizFlow.Core.Model;

namespace BizFlow.Core.Internal.Features.CancelPipeline
{
    public class CancelPipelineHandler : ICancelPipelineHandler
    {
        private readonly IPipelineService _pipelineService;
        private readonly ICancelPipelineRequestService _cancelPipelineRequestService;

        public CancelPipelineHandler(IPipelineService pipelineService, 
            ICancelPipelineRequestService cancelPipelineRequestService)
        {
            _pipelineService = pipelineService;
            _cancelPipelineRequestService = cancelPipelineRequestService;
        }

        public async Task<BizFlowChangingResult> CancelPipeline(CancelPipelineCommand command,
            CancellationToken cancellationToken = default)
        {
            var result = new BizFlowChangingResult() { Success = true };

            var pipelineNameExist = await _pipelineService.PipelineNameExist(command.PipelineName, cancellationToken);
            if (pipelineNameExist)
            {
                result.Success = false;
                result.Message = $"Пайплайна с именем: {command.PipelineName} не существует."; // TODO:i18n
                return result;
            }

            var addResult = await _cancelPipelineRequestService.AddAsync(new CancelPipelineRequest()
            {
                PipelineName = command.PipelineName,
                ExpirationTime = command.ExpirationTime,
                Description = command.Description,
                ClosingByExpirationTimeOnly = command.ClosingByExpirationTimeOnly,
                Created = DateTime.Now,
            }, cancellationToken);

            result.Message = addResult.ToString();

            return result;
        }
    }
}
