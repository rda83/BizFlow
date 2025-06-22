using BizFlow.Core.Contracts;
using BizFlow.Core.Internal.Shared;
using BizFlow.Core.Model;
using Microsoft.Extensions.DependencyInjection;

namespace BizFlow.Core.Internal.Features.AddPipeline
{
    internal class AddPipelineHandler : IAddPipelineHandler
    {
        private readonly BizFlowJobManager _bizFlowJobManager;
        private readonly IPipelineService _pipelineService;
        private readonly IServiceScopeFactory _scopeFactory;

        public AddPipelineHandler(BizFlowJobManager bizFlowJobManager,
            IPipelineService pipelineService, IServiceScopeFactory scopeFactory)
        {
            _bizFlowJobManager = bizFlowJobManager;
            _pipelineService = pipelineService;
            _scopeFactory = scopeFactory;
        }

        public async Task<BizFlowChangingResult> AddPipelineAsync(AddPipelineCommand command,
            CancellationToken cancellationToken = default)
        {
            var result = new BizFlowChangingResult() { Success = true };

            var pipelineNameExist = await _pipelineService.PipelineNameExist(command.Name, cancellationToken);
            if (pipelineNameExist)
            {
                result.Success = false;
                result.Message = $"Пайплайн с именем: {command.Name} уже существует."; // TODO:i18n
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
                result.Message = $"Обнаружены ошибки при проверке параметров элементов пайплайна."; // TODO:i18n
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

            await _pipelineService.AddPipelineAsync(pipeline, cancellationToken);
            await _bizFlowJobManager.CrerateTrigger(command.Name, command.CronExpression);

            return result;
        }
    }
}
