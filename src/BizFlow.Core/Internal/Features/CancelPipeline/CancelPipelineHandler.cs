using BizFlow.Core.Contracts;
using BizFlow.Core.Contracts.Storage;
using BizFlow.Core.Model;

namespace BizFlow.Core.Internal.Features.CancelPipeline
{
    public class CancelPipelineHandler : ICancelPipelineHandler
    {
        private readonly ICancelPipelineRequestService _cancelPipelineRequestService;
        private readonly IBizFlowStorage _storage;

        public CancelPipelineHandler(ICancelPipelineRequestService cancelPipelineRequestService,
            IBizFlowStorage storage)
        {
            _cancelPipelineRequestService = cancelPipelineRequestService;
            _storage = storage;
        }

        public async Task<BizFlowChangingResult> CancelPipeline(CancelPipelineCommand command,
            CancellationToken cancellationToken = default)
        {
            var result = new BizFlowChangingResult() { Success = true };

            var pipelineNameExist = await _storage.PipelineNameExistAsync(command.PipelineName, cancellationToken);
            if (!pipelineNameExist)
            {
                result.Success = false;
                result.Message = $"A pipeline with the name: {command.PipelineName} does not exist.";
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

        public async Task<BizFlowChangingResult> CloseCancellationRequest(CloseCancelPipelineCommand command, 
            CancellationToken cancellationToken = default)
        {
            var result = new BizFlowChangingResult() { Success = true };

            await _cancelPipelineRequestService.SetExecutedAsync(command.CancelPipeRequestId, command.Message, cancellationToken);
            return result;
        }
    }
}
