using BizFlow.Abstractions;
using BizFlow.Abstractions.Model;
using Microsoft.Extensions.DependencyInjection;

namespace BizFlow.Core
{
    public class PipelineExecutor
    {
        private readonly IServiceScopeFactory _scopeFactory;
        //private readonly PipelineExecutorJournal _journal;
        //private readonly CancellationMonitorService _cancellationMonitor;
        //private readonly IBizFlowStorage _storage;
        //private readonly ICancelPipelineHandler _cancelPipelineHandler;

        private readonly IExecutionJournal _journal;

        public PipelineExecutor(
            //IBizFlowStorage storage,
            IServiceScopeFactory scopeFactory,
            //PipelineExecutorJournal journal,
            //CancellationMonitorService cancellationMonitor,
            //ICancelPipelineHandler cancelPipelineHandler
            IExecutionJournal journal
            )
        {
            //_storage = storage;
            _scopeFactory = scopeFactory;
            _journal = journal;
            //_cancellationMonitor = cancellationMonitor;
            //_cancelPipelineHandler = cancelPipelineHandler;
        }

        public async Task Execute(Pipeline pipeline, CancellationToken ct) //IJobExecutionContext context
        {
            var launchId = Guid.NewGuid().ToString();
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(ct);

            //var triggerInfo = JobExecutionContextHelper.ExtractTriggerInfo(context);
            //var launchId = triggerInfo.LaunchId;
            //var pipelineName = triggerInfo.PipelineName;
            //var isStartNowPipeline = triggerInfo.IsStartNowPipeline;
            var isStartNowPipeline = false;

            //var pipeline = await _storage.GetPipelineAsync(pipelineName);

            
            if (pipeline == null)
            {
                await _journal.AddErrorAsync(launchId, isStartNowPipeline, $"The element for execution was not found: [TODO ...]");
                return;
            }

            pipeline!.Blocked = true; // DEBUG
            if (pipeline!.Blocked)
            {
                await _journal.AddBlockedPipelineAsync(launchId, isStartNowPipeline, pipeline.Name);
                return;
            }

            //var cancellationRequest = await _storage.GetActiveCancellationRequest(pipelineName);

            //if (cancellationRequest != null)
            //{
            //    foreach (var pipelineItem in pipeline.PipelineItems.OrderBy(i => i.SortOrder))
            //    {
            //        var cancelOperationArgs = new CancelOperationArgs()
            //        {
            //            LaunchId = launchId,
            //            PipelineName = pipelineName,
            //            ItemDescription = pipelineItem.Description,
            //            ItemId = pipelineItem.Id,
            //            ItemSortOrder = pipelineItem.SortOrder,
            //            TypeOperationId = pipelineItem.TypeOperationId,
            //            //Trigger = pipeline.CronExpression,
            //            IsStartNowPipeline = isStartNowPipeline,
            //            CancellationRequestId = cancellationRequest.Id,
            //        };
            //        await _journal.AddCanceled(cancelOperationArgs);
            //    }

            //    await CancellationRequestSetExecuted(cancellationRequest.Id,
            //        cancellationRequest.ClosingByExpirationTimeOnly);

            //    return;
            //}

            //var cancellationMonitoringTask = _cancellationMonitor.MonitorCancellationAsync(
            //    pipelineName, linkedCts, TimeSpan.FromSeconds(5));

            try
            {
                await ExecuteItems(pipeline, launchId, isStartNowPipeline, linkedCts.Token);   
            }
            finally
            {
                //linkedCts.Cancel();
                //var cancellationMonitoringResult = await cancellationMonitoringTask;
                //if (cancellationMonitoringResult != null)
                //{
                //    await CancellationRequestSetExecuted(cancellationMonitoringResult.CancellationRequestId,
                //        cancellationMonitoringResult.ClosingByExpirationTimeOnly);
                //}
            }
        }

        private async Task ExecuteItems(Pipeline pipeline, string launchId, bool isStartNowPipeline, 
            CancellationToken cancellationToken = default)
        {
            foreach (var pipelineItem in pipeline.PipelineItems.OrderBy(i => i.SortOrder))
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    await _journal.AddStartAsync(launchId, isStartNowPipeline, pipeline, pipelineItem);

                    if (pipelineItem.Blocked)
                    {
                //        await _journal.AddBlockedPipelineItem(launchId, pipeline, pipelineItem);
                        continue;
                    }

                    try
                    {
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var worker = scope.ServiceProvider
                                .GetRequiredKeyedService<IWorker>(pipelineItem.TypeOperationId);

                            var workerContext = new WorkerContext();
                            workerContext.LaunchId = launchId;
                            workerContext.TypeOperationId = pipelineItem.TypeOperationId ?? string.Empty;
                            workerContext.PipelineName = pipeline.Name;
                            //workerContext.CronExpression = pipeline.CronExpression;
                            //workerContext.CancellationToken = cancellationToken;
                            workerContext.Options = pipelineItem.Options;
                            workerContext.IsStartNowPipeline = isStartNowPipeline;

                            await worker.ExecuteAsync(workerContext, cancellationToken);
                        }
                        //        await _journal.AddSuccess(launchId, isStartNowPipeline, pipeline, pipelineItem);
                    }
                    catch (Exception)
                    {
                //        await _journal.AddError(launchId, isStartNowPipeline, pipeline, pipelineItem);
                        throw;
                    }
                }
                else
                {
                //    var cancelOperationArgs = new CancelOperationArgs()
                //    {
                //        LaunchId = launchId,
                //        PipelineName = pipeline.Name,
                //        ItemDescription = pipelineItem.Description,
                //        ItemId = pipelineItem.Id,
                //        ItemSortOrder = pipelineItem.SortOrder,
                //        TypeOperationId = pipelineItem.TypeOperationId,
                //        //Trigger = pipeline.CronExpression,
                //        IsStartNowPipeline = isStartNowPipeline,
                //    };
                //    await _journal.AddCanceled(cancelOperationArgs);
                }           
            }
        }
        
        private async Task CancellationRequestSetExecuted(long cancellationRequestId, bool closingByExpirationTimeOnly)
        {
            if (cancellationRequestId > 0 && !closingByExpirationTimeOnly)
            {
                //await _cancelPipelineHandler.CloseCancellationRequest(new CloseCancelPipelineCommand()
                //{
                //    CancelPipeRequestId = cancellationRequestId,
                //    Message = string.Empty,
                //});
            }
        }
    }
}
