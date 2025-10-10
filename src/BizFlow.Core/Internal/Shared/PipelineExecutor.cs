
using BizFlow.Core.Contracts;
using BizFlow.Core.Model;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace BizFlow.Core.Internal.Shared
{
    public class PipelineExecutor
    {
        private readonly IPipelineService _pipelineService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IBizFlowJournal _journal;
        private readonly ICancelPipelineRequestService _cancelPipelineRequestService;

        private const int DELAY_UPDATE_CANCELLATION_TOKEN = 5000;

        private CancellationRequestData _cancellationRequestData;

        public PipelineExecutor(IPipelineService pipelineService, IServiceScopeFactory scopeFactory,
            IBizFlowJournal journal, ICancelPipelineRequestService cancelPipelineRequestService)
        {
            _pipelineService = pipelineService;
            _scopeFactory = scopeFactory;
            _journal = journal;
            _cancelPipelineRequestService = cancelPipelineRequestService;

            _cancellationRequestData = new CancellationRequestData();
        }


        public async Task Execute(IJobExecutionContext context)
        {
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(context.CancellationToken);

            var triggerInfo = ExtractTriggerInfo(context);
            var launchId = triggerInfo.LaunchId;
            var pipelineName = triggerInfo.PipelineName;
            var isStartNowPipeline = triggerInfo.IsStartNowPipeline;

            var pipeline = await _pipelineService.GetPipelineAsync(pipelineName);
                
            if (pipeline == null)
            {
                await AddError(launchId, isStartNowPipeline, $"Не найден элемент для исполнения: {pipelineName}");//TODO i18n
                return;
            }
                              
            if (pipeline!.Blocked)
            {
                await AddBlockedPipeline(launchId, isStartNowPipeline, pipeline.Name);
                return;
            }

            var cancellationRequest = await _cancelPipelineRequestService.GetActiveRequest(pipelineName);

            if (cancellationRequest != null)
            {
                _cancellationRequestData.SetCancellationRequestData(cancellationRequest.ClosingByExpirationTimeOnly,
                    cancellationRequest.Description, cancellationRequest.Id);

                foreach (var pipelineItem in pipeline.PipelineItems.OrderBy(i => i.SortOrder))
                {
                    var cancelOperationArgs = new CancelOperationArgs()
                    {
                        LaunchId = launchId,
                        PipelineName = pipelineName,
                        ItemDescription = pipelineItem.Description,
                        ItemId = pipelineItem.Id,
                        ItemSortOrder = pipelineItem.SortOrder,
                        TypeOperationId = pipelineItem.TypeOperationId,
                        Trigger = pipeline.CronExpression,
                        IsStartNowPipeline = isStartNowPipeline,
                    };
                    await CancelOperation(cancelOperationArgs);
                }
                await CancellationRequestSetExecuted();
                return ;
            }

            var cancellationMonitoringTask = UpdateCancellationTokenSource(linkedCts, pipelineName);
            try
            {
                await ExecuteItems(pipeline, launchId, isStartNowPipeline, linkedCts.Token);   
            }
            finally
            {
                linkedCts.Cancel();
                await cancellationMonitoringTask;
            }
        }

        private (string LaunchId, string PipelineName, bool IsStartNowPipeline) ExtractTriggerInfo(IJobExecutionContext context)
        {
            var launchId = string.Empty;
            var pipelineName = string.Empty;
            var isStartNowPipeline = false;


            if (context.Trigger is Quartz.Impl.Triggers.CronTriggerImpl)
            {
                launchId = Guid.NewGuid().ToString();
                pipelineName = ((Quartz.Impl.Triggers.AbstractTrigger)context.Trigger).Name;
            }
            else if (context.Trigger is Quartz.Impl.Triggers.SimpleTriggerImpl)
            {
                var trigger = (Quartz.Impl.Triggers.SimpleTriggerImpl)context.Trigger;
                launchId = trigger.JobDataMap.GetString("launchId");
                pipelineName = trigger.JobDataMap.GetString("pipelineName");
                isStartNowPipeline = true;
            }
            else
            {
                throw new ArgumentException($"Unknown trigger type. [{context.Trigger.GetType()}]");
            }

            return (launchId, pipelineName, isStartNowPipeline);

        }

        private async Task ExecuteItems(Pipeline pipeline, string launchId, bool isStartNowPipeline, 
            CancellationToken cancellationToken = default)
        {
            foreach (var pipelineItem in pipeline.PipelineItems.OrderBy(i => i.SortOrder))
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    await AddStart(launchId, isStartNowPipeline, pipeline, pipelineItem)

                    if (pipelineItem.Blocked)
                    {
                        await AddBlockedPipelineItem(launchId, pipeline, pipelineItem);
                        continue;
                    }

                    try
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            IBizFlowWorker worker = scope.ServiceProvider
                                .GetRequiredKeyedService<IBizFlowWorker>(pipelineItem.TypeOperationId);

                            var workerContext = new WorkerContext();
                            workerContext.LaunchId = launchId;
                            workerContext.TypeOperationId = pipelineItem.TypeOperationId ?? string.Empty;
                            workerContext.PipelineName = pipeline.Name;
                            workerContext.CronExpression = pipeline.CronExpression;
                            workerContext.CancellationToken = cancellationToken;
                            workerContext.Options = pipelineItem.Options;
                            workerContext.IsStartNowPipeline = isStartNowPipeline;

                            await worker.Run(workerContext);
                        }
                        await AddSuccess(launchId, isStartNowPipeline, pipeline, pipelineItem);
                    }
                    catch (Exception)
                    {
                        await AddError(launchId, isStartNowPipeline, pipeline, pipelineItem);
                        throw;
                    }
                }
                else
                {
                    var cancelOperationArgs = new CancelOperationArgs()
                    {
                        LaunchId = launchId,
                        PipelineName = pipeline.Name,
                        ItemDescription = pipelineItem.Description,
                        ItemId = pipelineItem.Id,
                        ItemSortOrder = pipelineItem.SortOrder,
                        TypeOperationId = pipelineItem.TypeOperationId,
                        Trigger = pipeline.CronExpression,
                        IsStartNowPipeline = isStartNowPipeline,
                    };
                    await CancelOperation(cancelOperationArgs);
                    await CancellationRequestSetExecuted();
                }           
            }
        }

        private async Task UpdateCancellationTokenSource(CancellationTokenSource cts, string pipelineName)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var cancelPipelineRequestService = scope.ServiceProvider.GetRequiredService<ICancelPipelineRequestService>();
                while (!cts.IsCancellationRequested)
                {
                    await Task.Delay(DELAY_UPDATE_CANCELLATION_TOKEN);
                    var cancellationRequest = await cancelPipelineRequestService.GetActiveRequest(pipelineName);

                    if (cancellationRequest != null)
                    {
                        await cancelPipelineRequestService.SetExecutedAsync(cancellationRequest.Id);
                        cts.Cancel();
                    }
                }
            }
        }

        private async Task CancelOperation(CancelOperationArgs args)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var journal = scope.ServiceProvider.GetRequiredService<IBizFlowJournal>();

                var cancellationRequestData = _cancellationRequestData.GetCancellationRequestData();

                await AddCanceled(journal, args, cancellationRequestData.CancellationRequestId);
            }
        }

        private async Task CancellationRequestSetExecuted()
        {
            var cancellationRequestData = _cancellationRequestData.GetCancellationRequestData();

            if (cancellationRequestData.CancellationRequestId > 0
                && !cancellationRequestData.ClosingByExpirationTimeOnly)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var _cancelPipelineRequestService = scope.ServiceProvider
                        .GetRequiredService<ICancelPipelineRequestService>();

                    await _cancelPipelineRequestService.SetExecutedAsync(cancellationRequestData.CancellationRequestId);
                }
            }
        }

        private class CancelOperationArgs
        {
            public string LaunchId { get; set; } = string.Empty;
            public string PipelineName { get; set; } = string.Empty;
            public string ItemDescription { get; set; } = string.Empty;
            public int ItemSortOrder { get; set; }
            public long ItemId { get; set; }
            public string TypeOperationId { get; set; } = string.Empty;
            public string Trigger { get; set; } = string.Empty;
            public bool IsStartNowPipeline { get; set; }
        }

        private class CancellationRequestData
        {
            private bool _closingByExpirationTimeOnly;
            private string? _description = string.Empty;
            private long _cancellationRequestId;

            private object lockObject = new object();

            public void SetCancellationRequestData(bool closingByExpirationTimeOnly, string? description, long cancellationRequestId)
            {
                lock (lockObject)
                {
                    _closingByExpirationTimeOnly = closingByExpirationTimeOnly;
                    _description = description;
                    _cancellationRequestId = cancellationRequestId;

                }
            }

            public (bool ClosingByExpirationTimeOnly, string? Description, long CancellationRequestId) GetCancellationRequestData()
            {
                (bool ClosingByExpirationTimeOnly, string? Description, long CancellationRequestId) result;

                lock (lockObject)
                {
                    result.ClosingByExpirationTimeOnly = _closingByExpirationTimeOnly;
                    result.Description = _description;
                    result.CancellationRequestId = _cancellationRequestId;
                }
                return result;
            }
        }

        #region JournalAddRecord

        private async Task AddError(string launchId, bool isStartNowPipeline, string msg)
        {
            await _journal.AddRecordAsync(new BizFlowJournalRecord()
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
                IsStartNowPipeline = isStartNowPipeline,
            });
        }

        private async Task AddBlockedPipeline(string launchId, bool isStartNowPipeline, string pipelineName)
        {
            await _journal.AddRecordAsync(new BizFlowJournalRecord()
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
                IsStartNowPipeline = isStartNowPipeline,
            });
        }

        private async Task AddStart(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem)
        {
            await _journal.AddRecordAsync(new BizFlowJournalRecord()
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
                IsStartNowPipeline = isStartNowPipeline,
            });
        }

        private async Task AddBlockedPipelineItem(string launchId, Pipeline pipeline, PipelineItem pipelineItem)
        {
            await _journal.AddRecordAsync(new BizFlowJournalRecord()
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

        private async Task AddSuccess(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem)
        {
            await _journal.AddRecordAsync(new BizFlowJournalRecord()
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
                IsStartNowPipeline = isStartNowPipeline,
            });
        }

        private async Task AddError(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem)
        {
            await _journal.AddRecordAsync(new BizFlowJournalRecord()
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
                IsStartNowPipeline = isStartNowPipeline,
            });
        }

        private async Task AddCanceled(IBizFlowJournal journal, CancelOperationArgs args, long cancellationRequestId)
        {
            await journal.AddRecordAsync(new BizFlowJournalRecord()
            {
                Period = DateTime.Now,
                PipelineName = args.PipelineName,
                ItemDescription = args.ItemDescription,
                ItemSortOrder = args.ItemSortOrder,
                ItemId = args.ItemId,
                TypeAction = TypeBizFlowJournalAction.Canceled,
                TypeOperationId = args.TypeOperationId,
                LaunchId = args.LaunchId,
                Message = $"Операция отменена. Ид запроса на отмену: {cancellationRequestId}", //TODO i18n
                Trigger = args.Trigger,
                IsStartNowPipeline = args.IsStartNowPipeline,
            });
        }

        #endregion

    }
}
