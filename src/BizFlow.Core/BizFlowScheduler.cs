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

        // FOR DEBUG
        private bool tik = true;

        private readonly ITimeProvider _timeProvider;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPipelineRegistry _pipelineRegistry;
        private readonly ILogger<BizFlowScheduler> _logger;

        private readonly ConcurrentDictionary<string, DateTimeOffset?> _lastRunTimes = new();
        private readonly ConcurrentDictionary<string, bool> _runningStates = new();
        private readonly ConcurrentDictionary<string, DateTimeOffset?> _nextRunTimes = new();

        private readonly ConcurrentDictionary<string, CancellationTokenSource> _runningTokenSources = new();

        public BizFlowScheduler(
            IPipelineRegistry pipelineRegistry,
            IServiceScopeFactory scopeFactory,
            ILogger<BizFlowScheduler> logger,
            ITimeProvider timeProvider)
        {
            _pipelineRegistry = pipelineRegistry;
            _timeProvider = timeProvider;
            _scopeFactory = scopeFactory;
            _logger = logger;

            _pipelineRegistry.OnAdded += OnJobAdded;
            _pipelineRegistry.OnRemoved += OnJobRemoved;
        }

        private void OnJobAdded(object? sender, Pipeline pipelineDef)
        {
            _lastRunTimes.TryAdd(pipelineDef.Name, null);
            _runningStates.TryAdd(pipelineDef.Name, false);

            //var now = _timeProvider.UtcNow;
            //var nextRun = pipelineDef.Schedule.GetNextRun(null, now);
            
            // Возможно необходим пересчет следующего запуска
            _logger.LogInformation("!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
        }

        private void OnJobRemoved(object? sender, string pipelineName)
        {
            _lastRunTimes.TryRemove(pipelineName, out _);
            _runningStates.TryRemove(pipelineName, out _);
            // (?) _nextRunTimes
            //_logger.LogInformation("");
        }

        protected override async Task ExecuteAsync(CancellationToken appStoppingToken)
        {
            //var now = _timeProvider.UtcNow;
            foreach (var jobDef in _pipelineRegistry.GetAll())
            {
                _lastRunTimes.TryAdd(jobDef.Name, null);
                _runningStates.TryAdd(jobDef.Name, false);
                _nextRunTimes.TryAdd(jobDef.Name, null);
            }

            //_logger.LogInformation("Scheduler started. Tasks registered: {Count}.", _jobDefinitions.Count());

            _logger.LogInformation("Scheduler started.");

            //TODO: можно явно завернуть цикл в try-catch (OperationCanceledException)
            // и залогировать «Scheduler stopped by cancellation».

            while (!appStoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation(tik ? "tik" : "tak");
                tik = !tik;

                var now = _timeProvider.UtcNow;
                
                foreach (var jobDef in _pipelineRegistry.GetAll())
                {
                    _logger.LogInformation("EXECUTE");

                    if (appStoppingToken.IsCancellationRequested) break;

                    if (_nextRunTimes[jobDef.Name] == null) // TODO: проверить существует ли ключ
                    {
                        _nextRunTimes[jobDef.Name] = jobDef.Schedule.GetNextRun(_lastRunTimes[jobDef.Name], now);
                    }

                    DateTimeOffset? nextRun = _nextRunTimes[jobDef.Name];

                    if (nextRun.HasValue && nextRun.Value <= now)
                    {
                        if (_runningStates[jobDef.Name]) // TODO: проверить существует ли ключ
                        {
                            _logger.LogWarning("Task '{JobName}' is still running – execution skipped.", jobDef.Name);
                            continue;
                        }

                        _runningStates[jobDef.Name] = true;
                        _lastRunTimes[jobDef.Name] = now;

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
                        _nextRunTimes[jobDef.Name] = jobDef.Schedule.GetNextRun(_lastRunTimes[jobDef.Name], now);

                        _ = ExecuteJobAsync(jobDef, appStoppingToken);
                    }
                }
                await IntervalDelay(appStoppingToken);
            }
            _logger.LogInformation("Scheduler stopped.");
        }

        private async Task IntervalDelay(CancellationToken appStoppingToken)
        {
            // TODO: Проблема динамического добавления / удаления
            // Если задержка вычислена по старому набору(например, 5 минут до ближайшего запуска),
            // новая задача с более ранним временем не заставит цикл проснуться раньше – реакция задержится на всю длительность Task.Delay.

            //Использовать сигнализатор(например, ManualResetEventSlim или SemaphoreSlim),
            //который сбрасывается при добавлении задачи и позволяет мгновенно пересчитать расписание.
            
            //Либо перейти на таймер-ориентированный подход(PeriodicTimer или System.Threading.Timer)

            TimeSpan delay = TimeSpan.FromSeconds(DEFAULT_DELAY_INTERVAL_SECONDS);

            if(_nextRunTimes.Values.Count != 0 && !_nextRunTimes.Values.Where(i => i == null).Any())
            {
                var nextRun = _nextRunTimes.Values.OrderBy(i => i).FirstOrDefault();
                delay = (nextRun! - _timeProvider.UtcNow).Value;

                if (delay < TimeSpan.Zero)
                {
                    delay = TimeSpan.FromSeconds(DEFAULT_DELAY_INTERVAL_SECONDS);
                }
            }

            await Task.Delay(delay, appStoppingToken);
        }

        private async Task ExecuteJobAsync(Pipeline pipeline, CancellationToken appStoppingToken)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(appStoppingToken);
            _runningTokenSources.TryAdd(pipeline.Name, cts);

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
                _runningStates[pipeline.Name] = false;
                _runningTokenSources.TryRemove(pipeline.Name, out _);
            }
        }

        public bool Cancel(string pipelineName)
        {
            _logger.LogInformation($"Cancel pipeline: {pipelineName}");

            if (_runningTokenSources.TryGetValue(pipelineName, out var cts))
            {
                cts.Cancel();
                return true;
            }

            return false;
        }
    }
}
