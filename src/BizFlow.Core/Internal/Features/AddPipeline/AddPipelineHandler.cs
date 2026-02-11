using BizFlow.Core.Contracts;
using BizFlow.Core.Contracts.Storage;
using BizFlow.Core.Internal.Shared;
using BizFlow.Core.Model;
using Microsoft.Extensions.DependencyInjection;

namespace BizFlow.Core.Internal.Features.AddPipeline
{
    internal class AddPipelineHandler : IAddPipelineHandler
    {
        private readonly BizFlowJobManager _bizFlowJobManager;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IBizFlowStorage _storage;

        public AddPipelineHandler(BizFlowJobManager bizFlowJobManager, IServiceScopeFactory scopeFactory, IBizFlowStorage storage)
        {
            _bizFlowJobManager = bizFlowJobManager;
            _scopeFactory = scopeFactory;
            _storage = storage;
        }

        public async Task<BizFlowChangingResult> AddPipelineAsync(AddPipelineCommand command,
            CancellationToken cancellationToken = default)
        {
            var result = new BizFlowChangingResult() { Success = true };

            var pipelineNameExist = await _storage.PipelineNameExistAsync(command.Name, cancellationToken);
            if (pipelineNameExist)
            {
                result.Success = false;
                result.Message = $"A pipeline with the name: {command.Name} already exists.";
                return result;
            }

            // TODO: check command.CronExpression

            using (var scope = _scopeFactory.CreateScope())
            {
                foreach (var item in command.PipelineItems)
                {
                    IBizFlowWorker worker = scope.ServiceProvider
                        .GetRequiredKeyedService<IBizFlowWorker>(item.TypeOperationId);

                    var checkResult = await worker.CheckOptions(item.Options);

                    if (!checkResult.Success)
                    {
                        result.CheckItemsErrors.Add(new CheckItemsError()
                        {
                            TypeOperationId = item.TypeOperationId,
                            SortOrder = item.SortOrder,
                            Description = item.Description,
                            Message = checkResult.Message,
                        });
                    }
                }
            }

            if (result.CheckItemsErrors.Any())
            {
                result.Success = false;
                result.Message = $"Errors were found while validating pipeline item parameters.";
                return result;
            }

            var pipeline = new Pipeline(); // TODO: Builder
            pipeline.Name = command.Name;
            pipeline.CronExpression = command.CronExpression;
            pipeline.Description = command.Description;
            pipeline.Blocked = command.Blocked;
            pipeline.PipelineItems = command.PipelineItems.Select(i =>
            {
                var item = new PipelineItem();
                item.TypeOperationId = i.TypeOperationId;
                item.SortOrder = i.SortOrder;
                item.Description = i.Description;
                item.Blocked = i.Blocked;
                item.Options = i.Options;
                return item;
            }).ToList();
            
            await _storage.AddPipelineAsync(pipeline);
            await _bizFlowJobManager.CrerateTrigger(command.Name, command.CronExpression);

            return result;
        }
    }
}
