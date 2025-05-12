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

        public async Task<BizFlowChangingResult> AddPipelineAsync(AddPipelineCommand command,
            CancellationToken cancellationToken = default)
        {
            var result = new BizFlowChangingResult();

            // Существует ли операция
            // Проверка параметров


            var pipeline = new Pipeline();
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

            await pipelineService.AddPipelineAsync(pipeline, cancellationToken);


            // Создание триггера

            return result;
        }
    }
}
