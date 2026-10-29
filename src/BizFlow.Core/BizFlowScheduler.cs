using BizFlow.Abstractions;
using BizFlow.Abstractions.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace BizFlow.Core
{
    /// <summary>
    /// A background service that schedules and executes pipelines based on their defined schedules.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This service acts as the core engine of the library. It continuously evaluates registered pipelines,
    /// calculates their next run times, and launches them when they are due.
    /// </para>
    /// <para>
    /// The scheduler uses a semaphore as a wake-up signal to efficiently sleep when no pipelines are due,
    /// avoiding busy-waiting. It also tracks currently running tasks to ensure a graceful shutdown.
    /// </para>
    /// </remarks>
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

        /// <summary>
        /// Initializes a new instance of the <see cref="BizFlowScheduler"/> class.
        /// </summary>
        /// <param name="pipelineStateService">The service managing the runtime state of pipelines.</param>
        /// <param name="pipelineRegistry">The registry containing all pipeline definitions.</param>
        /// <param name="scopeFactory">The factory used to create DI scopes for pipeline execution.</param>
        /// <param name="logger">The logger instance.</param>
        /// <param name="timeProvider">The provider for the current time, allowing for testability.</param>
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

        /// <summary>
        /// Handles the event raised when a new pipeline is added to the registry.
        /// </summary>
        private void OnJobAdded(object? sender, Pipeline pipelineDef)
        {
            IntervalRelease(pipelineDef.Name);
            _logger.LogInformation("Added a new pipeline: '{PipelineName}'.", pipelineDef.Name); // LOG-OK
        }

        /// <summary>
        /// Handles the event raised when a pipeline is removed from the registry.
        /// </summary>
        private void OnJobRemoved(object? sender, string pipelineName)
        {
            IntervalRelease(pipelineName);
            _logger.LogInformation("Removed a pipeline: '{PipelineName}'.", pipelineName); // LOG-OK
        }

        /// <summary>
        /// Executes the main scheduling loop until the application is stopped.
        /// </summary>
        /// <param name="appStoppingToken">A token that is triggered when the application is shutting down.</param>
        protected override async Task ExecuteAsync(CancellationToken appStoppingToken)
        {
            _logger.LogInformation("Scheduler started."); // LOG-OK

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
            _logger.LogInformation("Scheduler stopped."); // LOG-OK
        }

        /// <summary>
        /// Delays the next scheduling iteration until the earliest next run time, a wake signal is received, or the application stops.
        /// </summary>
        /// <param name="appStoppingToken">A token to monitor for application shutdown.</param>
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

        /// <summary>
        /// Processes a single pipeline: checks its schedule and launches it if it is due.
        /// </summary>
        /// <param name="pipeline">The pipeline to process.</param>
        /// <param name="now">The current time at the start of processing this specific pipeline.</param>
        /// <param name="appStoppingToken">The application stopping token.</param>
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
                    _logger.LogDebug("Pipeline '{PipelineName}' is not due for execution yet.", pipeline.Name); // LOG-OK

                    if (!_pipelineStateService.TrySetWaiting(pipeline.Name))
                    {
                        _logger.LogWarning("Failed to reset state to Waiting for pipeline '{PipelineName}'. " + // LOG-OK
                            "The state might have been removed or replaced concurrently.", pipeline.Name);
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

        /// <summary>
        /// Executes the actual pipeline logic in a newly created DI scope.
        /// </summary>
        /// <param name="pipeline">The pipeline to execute.</param>
        /// <param name="now">The time when the execution was decided.</param>
        /// <param name="calculateNextRun">The calculated next run time.</param>
        /// <param name="appStoppingToken">The application stopping token.</param>
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
                _logger.LogInformation("Starting execution of pipeline '{PipelineName}'.", pipeline.Name); // LOG-OK

                using (var scope = _scopeFactory.CreateScope())
                {
                    var executor = scope.ServiceProvider.GetRequiredService<PipelineExecutor>();
                    await executor.Execute(pipeline, cts.Token);
                }

                _logger.LogInformation("Successfully finished execution of pipeline '{PipelineName}'.", pipeline.Name); // LOG-OK
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Execution of pipeline '{PipelineName}' was cancelled.", pipeline.Name); // LOG-OK
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Execution of pipeline '{PipelineName}' failed with an error.", pipeline.Name); // LOG-OK
            }
            finally
            {
                if (!_pipelineStateService.TryMarkCompleted(pipeline.Name)) // LOG-OK
                {
                    _logger.LogCritical(
                        "Invariant violation: failed to mark pipeline '{PipelineName}' as completed. " +
                        "The state might have been removed, replaced, or is not in InProgress state. " +
                        "The pipeline may be stuck.",
                        pipeline.Name);

                }
                IntervalRelease(pipeline.Name);
            }
        }

        /// <summary>
        /// Wakes up the scheduler from its sleep state.
        /// </summary>
        /// <param name="pipelineName">The name of the pipeline that triggered the wake-up (for logging).</param>
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
            }
        }

        /// <summary>
        /// Stops the scheduler, waits for running tasks to complete gracefully, and cleans up resources.
        /// </summary>
        /// <param name="appStoppingToken">The token indicating application shutdown.</param>
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

        /// <inheritdoc/>
        public bool Cancel(string pipelineName)
        {
            _logger.LogInformation("Cancel pipeline: '{PipelineName}'.", pipelineName); // LOG-OK
            return _pipelineStateService.TryCancel(pipelineName);
        }
    }
}
