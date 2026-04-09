using BizFlow.Core.Contracts.Storage;
using BizFlow.Core.Model;

namespace BizFlow.Core.Internal.Features.CancelPipeline
{
    public class CancelPipelineHandler : ICancelPipelineHandler
    {
        private readonly IBizFlowStorage _storage;

        public CancelPipelineHandler(IBizFlowStorage storage)
        {
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

            var addResult = await _storage.AddCancellationRequestAsync(new CancellationRequest()
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

            var cancellationRequest = await _storage.GetCancellationRequestAsync(command.CancelPipeRequestId, cancellationToken);

            if (cancellationRequest == null)
            {
                result.Success = false;
                result.Message = $"A cancellation request with the id: {command.CancelPipeRequestId} does not exist.";
                return result;
            }

            if (cancellationRequest.Executed)
            {
                result.Success = false;
                result.Message = $"A cancellation request with the id: {command.CancelPipeRequestId} has already been marked as executed.";
                return result;
            }

            cancellationRequest.Executed = true;
            cancellationRequest.ClosingTime = DateTime.Now;
            cancellationRequest.ClosedAfterExpirationDate =  cancellationRequest.ExpirationTime <= cancellationRequest.ClosingTime;

            if (!string.IsNullOrEmpty(command.Message))
            {
                cancellationRequest.Description = $"[{command.Message}]{cancellationRequest.Description}";
            }

            await _storage.UpdateCancellationRequestAsync(cancellationRequest);

            return result;
        }
    }
}
