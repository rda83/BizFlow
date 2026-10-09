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
            _wakeSignal.Release();
            _logger.LogInformation($"BizFlowScheduler: Added a new pipeline: {pipelineDef.Name}.");
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
                var now = _timeProvider.UtcNow;
                _logger.LogInformation(" #### ExecuteAsync #### Получено текущее время {now}", now);

                var pipelines = _pipelineRegistry.GetAll();

                _logger.LogInformation(" #### ExecuteAsync #### Получен список операций {Count}", pipelines.Count);

                int current = 0;
                foreach (var pipeline in pipelines)
                {
                    if (appStoppingToken.IsCancellationRequested) break;

                    current++;
                    _logger.LogInformation(" #### ExecuteAsync #### Начало обработки элемента {current} {Name}", current, pipeline.Name);

                    if (!_pipelineStateService.TrySetInProgress(pipeline.Name))
                    {
                        _logger.LogInformation(" #### ExecuteAsync #### Пропуск выполнения {Name}", pipeline.Name); // Нужно придумать сообщение
                        continue;
                    }

                    //_logger.LogInformation("EXECUTE"); // Нужно придумать сообщение


                    var nextRunTime = _pipelineStateService.GetNextRunTime(pipeline.Name);

                    _logger.LogInformation(" #### ExecuteAsync #### Получено время запуска {Name} {Time}", pipeline.Name, nextRunTime);
                  
                    if (nextRunTime == null) 
                    {
                        var lastRunTime = _pipelineStateService.GetLastRunTime(pipeline.Name);
                        var claculatedNextRun = pipeline.Schedule.GetNextRun(lastRunTime, now);

                        if(!_pipelineStateService.TrySetNextRunTime(pipeline.Name, claculatedNextRun))
                        {
                            continue;  // Ситуация аналогичная, если мы тут то что то сильно пошло не так
                        }
                        nextRunTime = claculatedNextRun;

                        _logger.LogInformation(" #### ExecuteAsync #### Вычислено время запуска {Name} {Time}", pipeline.Name, nextRunTime);
                    }

                    if (nextRunTime.HasValue && nextRunTime.Value <= now)
                    {
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

                        _logger.LogInformation(" #### ExecuteAsync #### Сработали условия выполнения {Name}", pipeline.Name);

                        var calculateNextRun = pipeline.Schedule.GetNextRun(now, now);

                        _logger.LogInformation(" #### ExecuteAsync #### Вычислено время следующего запуска {Name} {Time}", pipeline.Name, calculateNextRun);

                        _ = ExecuteJobAsync(pipeline, now, calculateNextRun, appStoppingToken);
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
                            continue;
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

                _logger.LogInformation(" #### IntervalDelay #### Следующая итерация {nextRun}", nextRun);

                delay = (nextRun! - _timeProvider.UtcNow).Value;

                if (delay < TimeSpan.Zero)
                {
                    delay = TimeSpan.FromSeconds(DEFAULT_DELAY_INTERVAL_SECONDS);
                }
            }

            _logger.LogInformation(" #### IntervalDelay #### Следующая итерация {delay}", delay);

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(appStoppingToken);
                
            var delayTask = Task.Delay(delay, linkedCts.Token);
            var wakeTask = _wakeSignal.WaitAsync(linkedCts.Token);

            var completedTask = await Task.WhenAny(delayTask, wakeTask);

            linkedCts.Cancel();
        }

        private async Task ExecuteJobAsync(Pipeline pipeline, DateTimeOffset? now, DateTimeOffset? calculateNextRun,
            CancellationToken appStoppingToken)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(appStoppingToken);
           
            if(!_pipelineStateService.TrySetBeforeStartedAttributes(pipeline.Name, now, calculateNextRun, cts))
            {
                // Здесь мы оказаться не  должны, нужно тоже что то делать
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
            }
        }

        public bool Cancel(string pipelineName)
        {
            _logger.LogInformation($"Cancel pipeline: {pipelineName}");
            return _pipelineStateService.TryCancel(pipelineName);
        }
    }
}
