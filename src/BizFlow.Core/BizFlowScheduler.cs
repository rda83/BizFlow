using BizFlow.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace BizFlow.Core
{
    public class BizFlowScheduler : BackgroundService
    {
        private readonly ITimeProvider _timeProvider;
        private readonly IEnumerable<JobDefinition> _jobDefinitions;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<BizFlowScheduler> _logger;

        private readonly Dictionary<JobDefinition, DateTimeOffset?> _lastRunTimes = new();
        private readonly Dictionary<JobDefinition, bool> _runningStates = new();


        public BizFlowScheduler(IEnumerable<JobDefinition> jobDefinitions,
            IServiceScopeFactory scopeFactory,
            ILogger<BizFlowScheduler> logger,
            ITimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
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
            _logger.LogInformation("Scheduler started. Tasks registered: {Count}.", _jobDefinitions.Count());
            

            while (!stoppingToken.IsCancellationRequested)
            {
                var now = _timeProvider.UtcNow;
                foreach (var jobDef in _jobDefinitions)
                {
                    if (stoppingToken.IsCancellationRequested) break;
               
                    var nextRun = jobDef.Schedule.GetNextRun(_lastRunTimes[jobDef], now);
                   
                    if (nextRun.HasValue && nextRun.Value <= now)
                    {
                        _lastRunTimes[jobDef] = now;
                        if (_runningStates[jobDef])
                        {
                            _logger.LogWarning("Task '{JobName}' is still running – execution skipped.", jobDef.Name);
                            continue;
                        }

                        _runningStates[jobDef] = true;

                        _ = ExecuteJobAsync(jobDef, stoppingToken);
                    }
                }

                // TODO Интервал опроса. В продакшене лучше вычислять время до ближайшего события.
                await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
            }
            _logger.LogInformation("Scheduler stopped.");
        }

        private async Task ExecuteJobAsync(JobDefinition jobDef, CancellationToken appStoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            // здесь можно получить контекст задания, если он зарегистрирован как Scoped

            try
            {
                _logger.LogInformation("Task '{JobName}' started execution.", jobDef.Name);

                await jobDef.Worker.ExecuteAsync(appStoppingToken);

                _logger.LogInformation("Task '{JobName}' completed successfully.", jobDef.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Task '{JobName}' failed with an error.", jobDef.Name);
            }
            finally
            {
                _runningStates[jobDef] = false;
            }
        }
    }
}
