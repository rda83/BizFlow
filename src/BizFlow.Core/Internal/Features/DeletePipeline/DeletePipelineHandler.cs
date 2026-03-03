using BizFlow.Core.Contracts.Storage;
using BizFlow.Core.Internal.Shared;
using BizFlow.Core.Model;

namespace BizFlow.Core.Internal.Features.DeletePipeline
{
    public class DeletePipelineHandler : IDeletePipelineHandler
    {
        private readonly BizFlowJobManager _bizFlowJobManager;
        private readonly IBizFlowStorage _storage;

        public DeletePipelineHandler(BizFlowJobManager bizFlowJobManager, IBizFlowStorage storage)
        {
            _bizFlowJobManager = bizFlowJobManager;
            _storage = storage;
        }

        public async Task<BizFlowChangingResult> DeletePipelineAsync(DeletePipelineCommand command, 
            CancellationToken cancellationToken = default)
        {
            var result = new BizFlowChangingResult() { Success = true };

            try
            {
                var pipelineNameExist = await _storage.PipelineNameExistAsync(command.Name, cancellationToken);
                if (!pipelineNameExist)
                {
                    result.Success = false;
                    result.Message = $"A pipeline with the name: {command.Name} does not exist.";
                }
                else
                {
                    await _storage.DeletePipelineAsync(command.Name, cancellationToken);
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
