using BizFlow.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace BizFlow.Core
{
    public class BizFlowScheduler : BackgroundService
    {
        private const int DEFAULT_DELAY_INTERVAL_SECONDS = 1;

        private readonly ITimeProvider _timeProvider;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BizFlowScheduler> _logger;

        private readonly ConcurrentDictionary<string, JobDefinition> _jobDefinitions = new();
        private readonly ConcurrentDictionary<string, DateTimeOffset?> _lastRunTimes = new();
        private readonly ConcurrentDictionary<string, bool> _runningStates = new();
        private readonly ConcurrentDictionary<string, DateTimeOffset?> _nextRunTimes = new();
        
        public BizFlowScheduler(IEnumerable<JobDefinition> jobDefinitions,
            IServiceScopeFactory scopeFactory,
            ILogger<BizFlowScheduler> logger,
            ITimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
            _scopeFactory = scopeFactory;
            _logger = logger;

            var now = _timeProvider.UtcNow;

            //TODO: будет необходима возможность динамического добавления/удаления задачи.

            foreach (var jobDef in jobDefinitions)
            {
                _runningStates[jobDef.Name] = false;
                _jobDefinitions[jobDef.Name] = jobDef;
                _lastRunTimes[jobDef.Name] = null;

                var nextRun = jobDef.Schedule.GetNextRun(null, now);
                _nextRunTimes[jobDef.Name] = nextRun;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Scheduler started. Tasks registered: {Count}.", _jobDefinitions.Count());
            
            //TODO: можно явно завернуть цикл в try-catch (OperationCanceledException)
            // и залогировать «Scheduler stopped by cancellation».

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = _timeProvider.UtcNow;
                
                foreach (var jobDef in _jobDefinitions.Values)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    if (_nextRunTimes[jobDef.Name] == null)
                    {
                        _nextRunTimes[jobDef.Name] = jobDef.Schedule.GetNextRun(_lastRunTimes[jobDef.Name], now);
                    }
                    DateTimeOffset? nextRun = _nextRunTimes[jobDef.Name];

                    if (nextRun.HasValue && nextRun.Value <= now)
                    {
                        if (_runningStates[jobDef.Name])
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

                        _ = ExecuteJobAsync(jobDef, stoppingToken);
                    }
                }
                await IntervalDelay(stoppingToken);
            }
            _logger.LogInformation("Scheduler stopped.");
        }

        private async Task IntervalDelay(CancellationToken stoppingToken)
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

            await Task.Delay(delay, stoppingToken);
        }

        private async Task ExecuteJobAsync(JobDefinition jobDef, CancellationToken appStoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            // здесь можно получить контекст задания, если он зарегистрирован как Scoped

            try
            {
                _logger.LogInformation("Task '{JobName}' started execution.", jobDef.Name);

                // Возможно необходим таймаут выполнения, например как параметр.

                await jobDef.Worker.ExecuteAsync(appStoppingToken);

                _logger.LogInformation("Task '{JobName}' completed successfully.", jobDef.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Task '{JobName}' failed with an error.", jobDef.Name);
            }
            finally
            {
                _runningStates[jobDef.Name] = false;
            }
        }
    }
}
