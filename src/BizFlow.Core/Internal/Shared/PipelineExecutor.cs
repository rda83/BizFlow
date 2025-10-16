
using BizFlow.Core.Contracts;
using BizFlow.Core.Internal.Shared.ExecutionServices;
using BizFlow.Core.Model;
using BizFlow.Core.Model.ExecutionServices;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace BizFlow.Core.Internal.Shared
{
    public class PipelineExecutor
    {
        private readonly IPipelineService _pipelineService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly PipelineExecutorJournal _journal;
        private readonly ICancelPipelineRequestService _cancelPipelineRequestService;
        private readonly CancellationMonitorService _cancellationMonitor;

        public PipelineExecutor(IPipelineService pipelineService, IServiceScopeFactory scopeFactory,
            PipelineExecutorJournal journal, ICancelPipelineRequestService cancelPipelineRequestService,
            CancellationMonitorService cancellationMonitor)
        {
            _pipelineService = pipelineService;
            _scopeFactory = scopeFactory;
            _journal = journal;
            _cancelPipelineRequestService = cancelPipelineRequestService;

            _cancellationMonitor = cancellationMonitor;
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
                await _journal.AddError(launchId, isStartNowPipeline, $"Не найден элемент для исполнения: {pipelineName}");//TODO i18n
                return;
            }
                              
            if (pipeline!.Blocked)
            {
                await _journal.AddBlockedPipeline(launchId, isStartNowPipeline, pipeline.Name);
                return;
            }

            var cancellationRequest = await _cancelPipelineRequestService.GetActiveRequest(pipelineName);

            if (cancellationRequest != null)
            {
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
                        CancellationRequestId = cancellationRequest.Id,
                    };
                    await _journal.AddCanceled(cancelOperationArgs);
                }
                await CancellationRequestSetExecuted(cancellationRequest.Id,
                    cancellationRequest.ClosingByExpirationTimeOnly);

                return;
            }

            var cancellationMonitoringTask = _cancellationMonitor.MonitorCancellationAsync(
                pipelineName, linkedCts, TimeSpan.FromSeconds(5));

            try
            {
                await ExecuteItems(pipeline, launchId, isStartNowPipeline, linkedCts.Token);   
            }
            finally
            {
                linkedCts.Cancel();
                var cancellationMonitoringResult = await cancellationMonitoringTask;
                if (cancellationMonitoringResult != null)
                {
                    await CancellationRequestSetExecuted(cancellationMonitoringResult.CancellationRequestId,
                        cancellationMonitoringResult.ClosingByExpirationTimeOnly);
                }
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
                    await _journal.AddStart(launchId, isStartNowPipeline, pipeline, pipelineItem);

                    if (pipelineItem.Blocked)
                    {
                        await _journal.AddBlockedPipelineItem(launchId, pipeline, pipelineItem);
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
                        await _journal.AddSuccess(launchId, isStartNowPipeline, pipeline, pipelineItem);
                    }
                    catch (Exception)
                    {
                        await _journal.AddError(launchId, isStartNowPipeline, pipeline, pipelineItem);
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
                    await _journal.AddCanceled(cancelOperationArgs);
                }           
            }
        }
        private async Task CancellationRequestSetExecuted(long cancellationRequestId, bool closingByExpirationTimeOnly)
        {
            if (cancellationRequestId > 0 && !closingByExpirationTimeOnly)
            {
                await _cancelPipelineRequestService.SetExecutedAsync(cancellationRequestId);
            }
        }
    }
}
