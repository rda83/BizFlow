
using BizFlow.Core.Contracts.Storage;
using BizFlow.Core.Model;
using BizFlow.Core.Model.ExecutionServices;

namespace BizFlow.Core.Internal.Shared.ExecutionServices
{
    public class PipelineExecutorJournal
    {
        private readonly IBizFlowStorage _bizFlowStorage;

        public PipelineExecutorJournal(IBizFlowStorage bizFlowStorage)
        {
            _bizFlowStorage = bizFlowStorage;
        }
        public async Task AddError(string launchId, bool isStartNowPipeline, string msg)
        {
            await _bizFlowStorage.AddJournalRecordAsync(new JournalRecord()
            {
                Period = DateTime.Now,
                PipelineName = string.Empty,
                ItemDescription = string.Empty,
                ItemSortOrder = 0,
                ItemId = 0,
                TypeAction = TypeBizFlowJournalAction.Error,
                TypeOperationId = string.Empty,
                LaunchId = launchId,
                Message = msg,
                Trigger = string.Empty,
                IsStartNow = isStartNowPipeline,
            });
        }
        public async Task AddBlockedPipeline(string launchId, bool isStartNowPipeline, string pipelineName)
        {
            await _bizFlowStorage.AddJournalRecordAsync(new JournalRecord()
            {
                Period = DateTime.Now,
                PipelineName = pipelineName,
                ItemDescription = string.Empty,
                ItemSortOrder = 0,
                ItemId = 0,
                TypeAction = TypeBizFlowJournalAction.BlockedPipeline,
                TypeOperationId = string.Empty,
                LaunchId = launchId,
                Message = string.Empty,
                Trigger = string.Empty,
                IsStartNow = isStartNowPipeline,
            });
        }
        public async Task AddStart(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem)
        {
            await _bizFlowStorage.AddJournalRecordAsync(new JournalRecord()
            {
                Period = DateTime.Now,
                PipelineName = pipeline.Name,
                ItemDescription = pipelineItem.Description,
                ItemSortOrder = pipelineItem.SortOrder,
                ItemId = pipelineItem.Id,
                TypeAction = TypeBizFlowJournalAction.Start,
                TypeOperationId = pipelineItem.TypeOperationId,
                LaunchId = launchId,
                Message = string.Empty,
                Trigger = pipeline.CronExpression,
                IsStartNow = isStartNowPipeline,
            });
        }
        public async Task AddBlockedPipelineItem(string launchId, Pipeline pipeline, PipelineItem pipelineItem)
        {
            await _bizFlowStorage.AddJournalRecordAsync(new JournalRecord()
            {
                Period = DateTime.Now,
                PipelineName = pipeline.Name,
                ItemDescription = pipelineItem.Description,
                ItemSortOrder = pipelineItem.SortOrder,
                ItemId = pipelineItem.Id,
                TypeAction = TypeBizFlowJournalAction.BlockedPipelineItem,
                TypeOperationId = pipelineItem.TypeOperationId,
                LaunchId = launchId,
                Message = string.Empty,
                Trigger = pipeline.CronExpression,
            });
        }
        public async Task AddSuccess(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem)
        {
            await _bizFlowStorage.AddJournalRecordAsync(new JournalRecord()
            {
                Period = DateTime.Now,
                PipelineName = pipeline.Name,
                ItemDescription = pipelineItem.Description,
                ItemSortOrder = pipelineItem.SortOrder,
                ItemId = pipelineItem.Id,
                TypeAction = TypeBizFlowJournalAction.Success,
                TypeOperationId = pipelineItem.TypeOperationId,
                LaunchId = launchId,
                Message = string.Empty,
                Trigger = pipeline.CronExpression,
                IsStartNow = isStartNowPipeline,
            });
        }
        public async Task AddError(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem)
        {
            await _bizFlowStorage.AddJournalRecordAsync(new JournalRecord()
            {
                Period = DateTime.Now,
                PipelineName = pipeline.Name,
                ItemDescription = pipelineItem.Description,
                ItemSortOrder = pipelineItem.SortOrder,
                ItemId = pipelineItem.Id,
                TypeAction = TypeBizFlowJournalAction.Error,
                TypeOperationId = pipelineItem.TypeOperationId,
                LaunchId = launchId,
                Message = string.Empty,
                Trigger = pipeline.CronExpression,
                IsStartNow = isStartNowPipeline,
            });
        }
        public async Task AddCanceled(CancelOperationArgs args)
        {
            await _bizFlowStorage.AddJournalRecordAsync(new JournalRecord()
            {
                Period = DateTime.Now,
                PipelineName = args.PipelineName,
                ItemDescription = args.ItemDescription,
                ItemSortOrder = args.ItemSortOrder,
                ItemId = args.ItemId,
                TypeAction = TypeBizFlowJournalAction.Canceled,
                TypeOperationId = args.TypeOperationId,
                LaunchId = args.LaunchId,
                Message = $"Операция отменена. Ид запроса на отмену: {args.CancellationRequestId}", //TODO i18n
                Trigger = args.Trigger,
                IsStartNow = args.IsStartNowPipeline,
            });
        }
    }
}
