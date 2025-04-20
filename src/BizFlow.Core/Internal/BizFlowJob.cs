using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BizFlow.Core.Internal
{

    // Необходим рефакторинг в соответствии с современным состоянием библиотеки Quartz.Net
    // !!! Quartz !!! Устарелость

    //_logger.LogInformation($"Scope HashCode: {_dbContext.GetHashCode()}");
    // При каждом выполнении будет разный HashCode для Scoped-сервиса.

    [DisallowConcurrentExecution] //возможно ли сделать два разных джоба?
    public class BizFlowJob : IJob
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IPipelineService _pipelineService;
        


        private bool _isFirstStart = true;

        public BizFlowJob(IPipelineService pipelineService, IServiceScopeFactory scopeFactory)
        {
            _pipelineService = pipelineService;
            _scopeFactory = scopeFactory;
        }

        //Scheduler - Возвращает экземпляр IScheduler, который выполняет задачу
        //Trigger - Возвращает экземпляр ITrigger, который вызвал выполнение задачи
        //JobDetail - Возвращает экземпляр IJobDetail, описывающий детали задачи
        //JobInstance - Возвращает экземпляр задачи(IJob), который выполняется
        //FireTimeUtc - Время, когда задача была запущена(в UTC)
        //NextFireTimeUtc - Время следующего запуска задачи(в UTC)
        //PreviousFireTimeUtc - Время предыдущего запуска задачи(в UTC)
        //Recovering - Показывает, выполняется ли задача в режиме восстановления
        //RefireCount - Количество повторных попыток выполнения(при сбое)
        //MergedJobDataMap - Объединенные данные из JobDataMap и Trigger(приоритет у Trigger)
        //Result - Результат выполнения задачи(может быть установлен в методе Execute)
        //CancellationToken - Токен отмены для прерывания выполнения задачи

        //Потокобезопасность: IJobExecutionContext создается для каждого выполнения задачи и не предназначен для совместного использования между потоками.
        //Данные: Данные можно передавать через JobDataMap, который доступен через MergedJobDataMap. При этом данные из триггера имеют приоритет над данными из задачи.
        //Результат: Свойство Result может использоваться для передачи результатов выполнения между задачами в цепочках.
        //Отмена:  CancellationToken позволяет корректно обрабатывать запросы на отмену выполнения задачи.

        public Task Execute(IJobExecutionContext context)
        {
            //IJobExecutionContext/CancellationToken


            if (_isFirstStart)
            {
                Task.Delay(60000).GetAwaiter().GetResult();
                _isFirstStart = false;
            }

            //using (var scope = _scopeFactory.CreateScope())
            //{
            //    var routineOperationRunner = scope.ServiceProvider.GetService<IRoutineOperationRunner>();

            //var pipelineService = scope.ServiceProvider.GetRequiredService<IPipelineService>();

            //var operationName = context.JobDetail.Key.Name;

            var tc = new TriggerContext();


            if (context.Trigger is Quartz.Impl.Triggers.CronTriggerImpl)
                {
                    //        if (!routineOpsServiceEnabled)
                    //            return Task.CompletedTask;

                    //        tc.LaunchId = Guid.NewGuid().ToString();
                    //        tc.RunUnblockedOnlyOps = RUN_UNBLOCKED_ONLY_OPS;

                            var triggerName = ((Quartz.Impl.Triggers.AbstractTrigger)context.Trigger).Name;
                            tc.TriggerName = triggerName;

                    //        var cronExpression = ((Quartz.Impl.Triggers.CronTriggerImpl)context.Trigger).CronExpressionString!;
                    //        tc.CronExpression = cronExpression;
                }
                else if (context.Trigger is Quartz.Impl.Triggers.SimpleTriggerImpl)
                {
                            var trigger = ((Quartz.Impl.Triggers.SimpleTriggerImpl)context.Trigger);
                            tc.TriggerName = trigger.Name;

                    //        tc.LaunchId = trigger.JobDataMap.GetString("launchId");
                    //        tc.CronExpression = trigger.JobDataMap.GetString("forTrigger");
                    //        tc.RunUnblockedOnlyOps = trigger.JobDataMap.GetBoolean("runUnblockedOnlyOps");
                }
                else
                {
                    throw new ArgumentException($"Unknown trigger type. [{context.Trigger.GetType()}]");
                }


                var pipeline = _pipelineService.GetPipeline(tc.TriggerName); //Теперь мы не создаем на каждый пайплайн свой джоб, он один. теперь триггер == пайплайн. 

            //1. Убрать комменты, сделать коммит.
            //2. Сделать дальнейший план разработки.
            //3. Сделать блок-схему с взаимодействием компонентов: пайплайны, воркеры, джобы, триггеры.

            foreach (var item in pipeline.PipelineItems)
                {
                    using (var scope = _scopeFactory.CreateScope())
                    {
                        IBizFlowWorker worker = scope.ServiceProvider.GetRequiredKeyedService<IBizFlowWorker>(item);
                        worker.Run(new WorkerContext());
                    }
                }

                // Механизм отмены операции из RoutineOps, возможно использовать штатный токен
                // Механизм отмены операции из BestPriceCalculationByStrategyWithSavingResultJob

                //RunPipeline(pipeline);
            //}
            return Task.CompletedTask;
        }
    }
}
