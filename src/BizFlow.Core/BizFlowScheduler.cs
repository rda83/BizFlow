using BizFlow.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BizFlow.Core
{
    public class BizFlowScheduler : BackgroundService
    {
        private readonly IEnumerable<JobDefinition> _jobDefinitions;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BizFlowScheduler> _logger;

        private readonly Dictionary<JobDefinition, DateTimeOffset?> _lastRunTimes = new();
        private readonly Dictionary<JobDefinition, bool> _runningStates = new();


        public BizFlowScheduler(IEnumerable<JobDefinition> jobDefinitions,
            IServiceScopeFactory scopeFactory,
            ILogger<BizFlowScheduler> logger)
        {
            _jobDefinitions = jobDefinitions;
            _scopeFactory = scopeFactory;
            _logger = logger;

            foreach (var def in _jobDefinitions)
            {
                _lastRunTimes[def] = null;
                _runningStates[def] = false;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Планировщик запущен. Зарегистрировано задач: {Count}", _jobDefinitions.Count()); // TODO перевод сообщения

            while (!stoppingToken.IsCancellationRequested)
            {
                

                foreach (var jobDef in _jobDefinitions)
                {
                    if (stoppingToken.IsCancellationRequested) break;

                    var nextRun = jobDef.Schedule.GetNextRun(_lastRunTimes[jobDef]);
                    var now = DateTimeOffset.UtcNow;
                    if (nextRun.HasValue && nextRun.Value <= now)
                    {
                        _lastRunTimes[jobDef] = now;
                        if (_runningStates[jobDef])
                        {
                            _logger.LogWarning("Задача '{JobName}' ещё выполняется – запуск пропущен", jobDef.Name); // TODO перевод сообщения
                            continue;
                        }

                        _runningStates[jobDef] = true;

                        _ = ExecuteJobAsync(jobDef, stoppingToken);
                    }
                }

                // TODO Интервал опроса. В продакшене лучше вычислять время до ближайшего события.
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            _logger.LogInformation("Планировщик остановлен");
        }

        private async Task ExecuteJobAsync(JobDefinition jobDef, CancellationToken appStoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            // здесь можно получить контекст задания, если он зарегистрирован как Scoped

            try
            {
                _logger.LogInformation("Задача '{JobName}' начала выполнение", jobDef.Name); // TODO перевод сообщения

                await jobDef.Worker.ExecuteAsync(appStoppingToken);

                _logger.LogInformation("Задача '{JobName}' успешно завершена", jobDef.Name); // TODO перевод сообщения
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Задача '{JobName}' завершилась с ошибкой", jobDef.Name); // TODO перевод сообщения
            }
            finally
            {
                _runningStates[jobDef] = false;
            }
        }
    }
}
