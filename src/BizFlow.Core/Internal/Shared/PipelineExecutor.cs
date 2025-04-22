
using BizFlow.Core.Contracts;
using BizFlow.Core.Model;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace BizFlow.Core.Internal.Shared
{
    //IJobExecutionContext
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


    public class PipelineExecutor
    {
        private readonly IPipelineService _pipelineService;
        private readonly IServiceScopeFactory _scopeFactory;

        public PipelineExecutor(IPipelineService pipelineService, IServiceScopeFactory scopeFactory)
        {
            _pipelineService = pipelineService;
            _scopeFactory = scopeFactory;
        }

        public Task Execute(IJobExecutionContext context)
        {
            //IJobExecutionContext/CancellationToken

            var triggerName = string.Empty;
            if (context.Trigger is Quartz.Impl.Triggers.CronTriggerImpl)
            {
                //        if (!routineOpsServiceEnabled)
                //            return Task.CompletedTask;

                //        tc.LaunchId = Guid.NewGuid().ToString();
                //        tc.RunUnblockedOnlyOps = RUN_UNBLOCKED_ONLY_OPS;

                triggerName = ((Quartz.Impl.Triggers.AbstractTrigger)context.Trigger).Name;

                //        var cronExpression = ((Quartz.Impl.Triggers.CronTriggerImpl)context.Trigger).CronExpressionString!;
                //        tc.CronExpression = cronExpression;
            }
            else if (context.Trigger is Quartz.Impl.Triggers.SimpleTriggerImpl)
            {
                var trigger = (Quartz.Impl.Triggers.SimpleTriggerImpl)context.Trigger;
                triggerName = trigger.Name;

                //        tc.LaunchId = trigger.JobDataMap.GetString("launchId");
                //        tc.CronExpression = trigger.JobDataMap.GetString("forTrigger");
                //        tc.RunUnblockedOnlyOps = trigger.JobDataMap.GetBoolean("runUnblockedOnlyOps");
            }
            else
            {
                throw new ArgumentException($"Unknown trigger type. [{context.Trigger.GetType()}]");
            }

            var pipeline = _pipelineService.GetPipeline(triggerName);

            foreach (var item in pipeline.PipelineItems.OrderBy(i => i.SortOrder))
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    IBizFlowWorker worker = scope.ServiceProvider
                        .GetRequiredKeyedService<IBizFlowWorker>(item.TypeOperationId);

                    worker.Run(new WorkerContext());
                }
            }

            return Task.CompletedTask;
        }
    }
}
