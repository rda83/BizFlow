using BizFlow.Abstractions;
using BizFlow.Abstractions.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace BizFlow.Core
{
    public class BizFlowScheduler : BackgroundService, ICancellationService
    {
        private const int DEFAULT_DELAY_INTERVAL_SECONDS = 1;

        private readonly PipelineStateService _pipelineStateService;
        private readonly ITimeProvider _timeProvider;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPipelineRegistry _pipelineRegistry;
        private readonly ILogger<BizFlowScheduler> _logger;
        private readonly ConcurrentDictionary<string, Task> _runningTasks = new();


        private readonly SemaphoreSlim _wakeSignal = new SemaphoreSlim(0, 1);

        public BizFlowScheduler(
            PipelineStateService pipelineStateService,
            IPipelineRegistry pipelineRegistry,
            IServiceScopeFactory scopeFactory,
            ILogger<BizFlowScheduler> logger,
            ITimeProvider timeProvider)
        {
            _pipelineStateService = pipelineStateService;
            _pipelineRegistry = pipelineRegistry;
            _timeProvider = timeProvider;
            _scopeFactory = scopeFactory;
            _logger = logger;

            _pipelineRegistry.OnAdded += OnJobAdded;
            _pipelineRegistry.OnRemoved += OnJobRemoved;
        }

        private void OnJobAdded(object? sender, Pipeline pipelineDef)
        {
            IntervalRelease(pipelineDef.Name);
            _logger.LogInformation("Added a new pipeline: '{PipelineName}'.", pipelineDef.Name); // LOG-OK
        }

        private void OnJobRemoved(object? sender, string pipelineName)
        {
            _logger.LogInformation($"BizFlowScheduler: Removed a pipeline: {pipelineName}.");
        }

        protected override async Task ExecuteAsync(CancellationToken appStoppingToken)
        {
            _logger.LogInformation("Scheduler started.");

            while (!appStoppingToken.IsCancellationRequested)
            {
               
                var pipelines = _pipelineRegistry.GetAll();

                foreach (var pipeline in pipelines)
                {
                    if (appStoppingToken.IsCancellationRequested) break;
                    var now = _timeProvider.UtcNow;

                    await ProcessPipelineAsync(pipeline, now, appStoppingToken);
                }

                await IntervalDelay(appStoppingToken);
            }
            _logger.LogInformation("Scheduler stopped.");
        }

        private async Task IntervalDelay(CancellationToken appStoppingToken)
        {
            TimeSpan delay = Timeout.InfiniteTimeSpan;
            var nextRunTimesSnapshot = _pipelineStateService.GetWaitingItemsNextNotNullRunTimes();

            if (nextRunTimesSnapshot.Any())
            {
                var nextRun = nextRunTimesSnapshot.Min();
                _logger.LogInformation("Next scheduled run at {NextRun}", nextRun); // LOG-OK

                delay = nextRun - _timeProvider.UtcNow;

                if (delay < TimeSpan.Zero)
                {
                    _logger.LogWarning( // LOG-OK
                        "IntervalDelay: waiting pipeline with past NextRunTime (min={NextRun}). " +
                        "Falling back to {Delay} before retry.",
                        nextRun,
                        TimeSpan.FromSeconds(DEFAULT_DELAY_INTERVAL_SECONDS));

                    delay = TimeSpan.FromSeconds(DEFAULT_DELAY_INTERVAL_SECONDS);
                }
            }

            _logger.LogInformation("Sleeping for {Delay}", delay); // LOG-OK

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(appStoppingToken);
            var delayTask = Task.Delay(delay, linkedCts.Token);
            var wakeTask = _wakeSignal.WaitAsync(linkedCts.Token);
            var completedTask = await Task.WhenAny(delayTask, wakeTask);

            linkedCts.Cancel();
            try
            {
                await Task.WhenAll(delayTask, wakeTask);
            }
            catch (OperationCanceledException)
            {
                _logger.LogDebug(
                    "IntervalDelay: expected cancellation of the remaining wait task after wake-up."); // LOG-OK
            }
        }

        private async Task ProcessPipelineAsync(
            Pipeline pipeline,
            DateTimeOffset now,
            CancellationToken appStoppingToken)
        {
            if (!_pipelineStateService.TrySetInProgress(pipeline.Name))
            {
                return;
            }

            bool taskLaunched = false;

            try
            {
                var nextRunTime = _pipelineStateService.GetNextRunTime(pipeline.Name);
                if (nextRunTime == null)
                {
                    var lastRunTime = _pipelineStateService.GetLastRunTime(pipeline.Name);
                    var claculatedNextRun = pipeline.Schedule.GetNextRun(lastRunTime, now);

                    if (!_pipelineStateService.TrySetNextRunTime(pipeline.Name, claculatedNextRun))
                    {
                        _logger.LogCritical( // LOG-OK
                            "Invariant violation: TrySetNextRunTime failed for pipeline {Name}. " +
                            "State was removed or replaced between TrySetInProgress and now.",
                            pipeline.Name);

                        _pipelineStateService.TrySetWaiting(pipeline.Name);

                        return;
                    }
                    nextRunTime = claculatedNextRun;
                }

                if (nextRunTime.HasValue && nextRunTime.Value <= now)
                {
                    // TODO: сейчас считаем от now - вычисленного до запуска задачи.
                    // Если задача выполняется очень долго, к моменту её завершения это время может уже пройти.
                    // Какие есть варианты:
                    //  - оставить как есть;
                    //  - считать от времени фактического завершения;
                    //  - параметризировать поведение.
                    // Стоит чётко задокументировать или параметризовать:
                    //  NextRunAfterStart(как сейчас)
                    //  NextRunAfterCompletion(гарантирует интервал между задачами)
                    //  CatchUp(запустить пропущенные сразу после завершения).

                    var calculateNextRun = pipeline.Schedule.GetNextRun(now, now);
                    var runningTask = ExecuteJobAsync(pipeline, now, calculateNextRun, appStoppingToken);
                    taskLaunched = true;

                    if (_runningTasks.TryAdd(pipeline.Name, runningTask))
                    {
                        _ = runningTask.ContinueWith(t => _runningTasks.TryRemove(new KeyValuePair<string, Task>(pipeline.Name, t)),
                            CancellationToken.None,
                            TaskContinuationOptions.ExecuteSynchronously,
                            TaskScheduler.Default);
                    }
                    else
                    {
                        _logger.LogCritical("Duplicate running task for pipeline {Name}. State machine invariant violated.", // LOG-OK
                            pipeline.Name);
                    }
                }
                else
                {
                    // Тут просто условия запуска не сработали, и нам надо сбросить флаг того что операция выполняется
                    _logger.LogInformation(" #### ExecuteAsync #### Несработали условия выполнения {Name}", pipeline.Name);

                    if (_pipelineStateService.TrySetWaiting(pipeline.Name))
                    {
                        _logger.LogInformation(" #### ExecuteAsync #### Сброшен флаг выполнения {Name}", pipeline.Name);
                    }
                    else
                    {
                        _logger.LogInformation(" #### ExecuteAsync #### Ошибка сброса флага выполнения выполнения {Name}", pipeline.Name);
                    }
                    return;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to schedule pipeline {Name}.", pipeline.Name); // LOG-OK
            }
            finally
            {
                if (!taskLaunched)
                {
                    _pipelineStateService.TrySetWaiting(pipeline.Name);
                }
            }
        }

        private async Task ExecuteJobAsync(Pipeline pipeline, DateTimeOffset? now, DateTimeOffset? calculateNextRun,
            CancellationToken appStoppingToken)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(appStoppingToken);
           
            if(!_pipelineStateService.TrySetBeforeStartedAttributes(pipeline.Name, now, calculateNextRun, cts))
            {
                _logger.LogCritical( // LOG-OK
                    "Invariant violation: TrySetBeforeStartedAttributes failed for pipeline {Name}. " +
                    "State is not InProgress or has been replaced. Execution skipped.",
                    pipeline.Name);

                return;
            }

            try
            {
                //_logger.LogInformation("Task '{JobName}' started execution.", pipeline.Name);

                _logger.LogInformation(" #### ExecuteJobAsync #### ЗАПУСК {Name}", pipeline.Name);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var executor = scope.ServiceProvider.GetRequiredService<PipelineExecutor>();
                    await executor.Execute(pipeline, cts.Token);
                }

                _logger.LogInformation(" #### ExecuteJobAsync #### ЗАВЕРШЕНИЕ {Name}", pipeline.Name);

                //_logger.LogInformation("Task '{JobName}' completed successfully.", pipeline.Name);
            }
            catch (OperationCanceledException)
            {
                //_logger.LogInformation("Task '{JobName}' cancelled.", pipeline.Name);
            }
            catch (Exception ex)
            {
                //_logger.LogError(ex, "Task '{JobName}' failed with an error.", pipeline.Name);
            }
            finally
            {
                _logger.LogInformation(" #### ExecuteJobAsync #### TryMarkCompleted < {Name}", pipeline.Name);

                if (!_pipelineStateService.TryMarkCompleted(pipeline.Name))
                {
                    // Нужно подумать что делаем в этом месте

                    _logger.LogInformation(" #### ExecuteJobAsync #### TryMarkCompleted ERROR {Name}", pipeline.Name);

                }

                _logger.LogInformation(" #### ExecuteJobAsync #### TryMarkCompleted > {Name}", pipeline.Name);

                IntervalRelease(pipeline.Name);
            }
        }

        private void IntervalRelease(string pipelineName)
        {
            try
            {
                _wakeSignal.Release();
            }
            catch (SemaphoreFullException)
            {
                _logger.LogDebug( // LOG-OK
                    "Wake signal already pending; coalescing with new pipeline '{PipelineName}'.",
                    pipelineName);
                throw;
            }
        }

        public override async Task StopAsync(CancellationToken appStoppingToken)
        {
            await base.StopAsync(appStoppingToken);
            var tasks = _runningTasks.Values.ToArray();

            if (tasks.Length > 0)
            {
                try 
                { 
                    await Task.WhenAll(tasks).WaitAsync(appStoppingToken);
                }
                catch (OperationCanceledException) {}
            }

            _pipelineRegistry.OnAdded -= OnJobAdded;
            _pipelineRegistry.OnRemoved -= OnJobRemoved;

            _wakeSignal.Dispose();
        }

        public bool Cancel(string pipelineName)
        {
            _logger.LogInformation($"Cancel pipeline: {pipelineName}");
            return _pipelineStateService.TryCancel(pipelineName);
        }
    }
}
