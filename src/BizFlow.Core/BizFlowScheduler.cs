using BizFlow.Abstractions;
using BizFlow.Abstractions.Model;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
            _pipelineStateService.Add(pipelineDef.Name);
            _wakeSignal.Release();
            _logger.LogInformation($"BizFlowScheduler: Added a new pipeline: {pipelineDef.Name}.");
        }

        private void OnJobRemoved(object? sender, string pipelineName)
        {
            _pipelineStateService.Remove(pipelineName);
            _logger.LogInformation($"BizFlowScheduler: Removed a pipeline: {pipelineName}.");
        }

        protected override async Task ExecuteAsync(CancellationToken appStoppingToken)
        {
            foreach (var pipelineDef in _pipelineRegistry.GetAll())
            {
                _pipelineStateService.Add(pipelineDef.Name);
            }

            _logger.LogInformation("Scheduler started.");

            //TODO: можно явно завернуть цикл в try-catch (OperationCanceledException)
            // и залогировать «Scheduler stopped by cancellation».

            while (!appStoppingToken.IsCancellationRequested)
            {
                var now = _timeProvider.UtcNow;
                
                foreach (var jobDef in _pipelineRegistry.GetAll())
                {
                    _logger.LogInformation("EXECUTE");

                    if (appStoppingToken.IsCancellationRequested) break;

                    if (_pipelineStateService.GetNextRunTime(jobDef.Name) == null) // TODO: проверить существует ли ключ
                    {
                        _pipelineStateService.SetNextRunTime(jobDef.Name, 
                            jobDef.Schedule.GetNextRun(_pipelineStateService.GetLastRunTime(jobDef.Name), now));
                    }

                    DateTimeOffset? jobRunTime = _pipelineStateService.GetNextRunTime(jobDef.Name);

                    if (jobRunTime.HasValue && jobRunTime.Value <= now)
                    {
                        if (_pipelineStateService.IsRunning(jobDef.Name)) // TODO: проверить существует ли ключ
                        {
                            _logger.LogWarning("Task '{JobName}' is still running – execution skipped.", jobDef.Name);
                            continue;
                        }

                        // TODO: сейчас считаем от now - вычесленного до запуска задачи.
                        // Если задача выполняется очень долго, к моменту её завершения это время может уже пройти.
                        // Какие есть варианты:
                        //  - оставить как есть;
                        //  - считать от времени фактического завершения;
                        //  - параметризировать поведение.
                        // Стоит чётко задокументировать или параметризовать:
                        //  NextRunAfterStart(как сейчас)
                        //  NextRunAfterCompletion(гарантирует интервал между задачами)
                        //  CatchUp(запустить пропущенные сразу после завершения).

                        // TODO: Если Schedule.GetNextRun - возвращает null
                        // далее на каждой итерации будет так же получать null, IntervalDelay падает в дефолтную секунду
                        // возможно такие задачи необходимо как то отмечать.

                        var nextRun = jobDef.Schedule.GetNextRun(now, now);
                        _ = ExecuteJobAsync(jobDef, now, nextRun, appStoppingToken);
                    }
                }
                await IntervalDelay(appStoppingToken);
            }
            _logger.LogInformation("Scheduler stopped.");
        }

        private async Task IntervalDelay(CancellationToken appStoppingToken)
        {
            TimeSpan delay = TimeSpan.FromSeconds(DEFAULT_DELAY_INTERVAL_SECONDS);

            var nextRunTimesSnapshot = _pipelineStateService.GetAllNextRunTimes();

            if (nextRunTimesSnapshot.Count() != 0 && !nextRunTimesSnapshot.Where(i => i == null).Any())
            {
                var nextRun = nextRunTimesSnapshot.OrderBy(i => i).FirstOrDefault();
                delay = (nextRun! - _timeProvider.UtcNow).Value;

                if (delay < TimeSpan.Zero)
                {
                    delay = TimeSpan.FromSeconds(DEFAULT_DELAY_INTERVAL_SECONDS);
                }
            }

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(appStoppingToken);
                
            var delayTask = Task.Delay(delay, linkedCts.Token);
            var wakeTask = _wakeSignal.WaitAsync(linkedCts.Token);

            var completedTask = await Task.WhenAny(delayTask, wakeTask);

            linkedCts.Cancel();
        }

        private async Task ExecuteJobAsync(Pipeline pipeline, DateTimeOffset? now, DateTimeOffset? nextRun,
            CancellationToken appStoppingToken)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(appStoppingToken);

            _pipelineStateService.MarkStarted(pipeline.Name, now, nextRun, cts);

            try
            {
                _logger.LogInformation("Task '{JobName}' started execution.", pipeline.Name);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var executor = scope.ServiceProvider.GetRequiredService<PipelineExecutor>();
                    await executor.Execute(pipeline, cts.Token);
                }

                _logger.LogInformation("Task '{JobName}' completed successfully.", pipeline.Name);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Task '{JobName}' cancelled.", pipeline.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Task '{JobName}' failed with an error.", pipeline.Name);
            }
            finally
            {
                // TODO: Если пока выполнялась задача - она была удалена
                _pipelineStateService.MarkCompleted(pipeline.Name);
            }
        }

        public void Cancel(string pipelineName)
        {
            _logger.LogInformation($"Cancel pipeline: {pipelineName}");
            _pipelineStateService.Cancel(pipelineName);
        }
    }
}
