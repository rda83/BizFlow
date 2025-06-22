
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

        public async Task Execute(IJobExecutionContext context)
        {
            //IJobExecutionContext/CancellationToken

            var triggerName = string.Empty;
            if (context.Trigger is Quartz.Impl.Triggers.CronTriggerImpl)
            {
                triggerName = ((Quartz.Impl.Triggers.AbstractTrigger)context.Trigger).Name;
            }
            else if (context.Trigger is Quartz.Impl.Triggers.SimpleTriggerImpl)
            {
                var trigger = (Quartz.Impl.Triggers.SimpleTriggerImpl)context.Trigger;
                triggerName = trigger.Name;
            }
            else
            {
                throw new ArgumentException($"Unknown trigger type. [{context.Trigger.GetType()}]");
            }

            try
            {
                var pipeline = await _pipelineService.GetPipelineAsync(triggerName); // TODO: check null

                if (pipeline == null)
                {
                    // TODO Журнал, логирование
                    return;
                }
                              
                if (pipeline!.Blocked)
                {
                    // TODO Журнал, логирование
                    return;
                }


                var launchId = Guid.NewGuid().ToString();
                foreach (var pipelineItem in pipeline.PipelineItems.OrderBy(i => i.SortOrder))
                {
                    if (pipelineItem.Blocked)
                    {
                        // TODO Журнал, логирование
                        continue;
                    }

                    using (var scope = _scopeFactory.CreateScope())
                    {
                        IBizFlowWorker worker = scope.ServiceProvider
                            .GetRequiredKeyedService<IBizFlowWorker>(pipelineItem.TypeOperationId);

                        var workerContext = new WorkerContext();
                        workerContext.LaunchId = launchId;
                        workerContext.TypeOperationId = pipelineItem.TypeOperationId ?? string.Empty;
                        workerContext.PipelineName = pipeline.Name;
                        workerContext.CronExpression = pipeline.CronExpression;
                        workerContext.CancellationToken = context.CancellationToken;
                        workerContext.Options = pipelineItem.Options;

                        // TODO Исключение может возникнуть здесь
                        await worker.Run(workerContext);
                    }
                }
            }
            catch (Exception)
            {
                // TODO Журнал, логирование
                throw;
            }
        }
    }
}
