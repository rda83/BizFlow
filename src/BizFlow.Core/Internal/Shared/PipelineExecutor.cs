﻿
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
        private readonly IBizFlowJournal _journal;

        public PipelineExecutor(IPipelineService pipelineService, IServiceScopeFactory scopeFactory,
            IBizFlowJournal journal)
        {
            _pipelineService = pipelineService;
            _scopeFactory = scopeFactory;
            _journal = journal;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            //IJobExecutionContext/CancellationToken
            string launchId = string.Empty;
            string pipelineName = string.Empty;
            bool isStartNowPipeline = false;

            if (context.Trigger is Quartz.Impl.Triggers.CronTriggerImpl)
            {
                launchId = Guid.NewGuid().ToString();
                pipelineName = ((Quartz.Impl.Triggers.AbstractTrigger)context.Trigger).Name;
            }
            else if (context.Trigger is Quartz.Impl.Triggers.SimpleTriggerImpl)
            {
                var trigger = (Quartz.Impl.Triggers.SimpleTriggerImpl)context.Trigger;
                launchId = trigger.JobDataMap.GetString("launchId");
                pipelineName = trigger.JobDataMap.GetString("pipelineName");
                isStartNowPipeline = true;
            }
            else
            {
                throw new ArgumentException($"Unknown trigger type. [{context.Trigger.GetType()}]");
            }

            try
            {
                var pipeline = await _pipelineService.GetPipelineAsync(pipelineName);
                
                if (pipeline == null)
                {
                    await _journal.AddRecordAsync(new BizFlowJournalRecord()
                    {
                        Period = DateTime.Now,
                        PipelineName = string.Empty,
                        ItemDescription = string.Empty,
                        ItemSortOrder = 0,
                        TypeAction = TypeBizFlowJournaAction.Error,
                        TypeOperationId = string.Empty,
                        LaunchId = launchId,
                        Message = $"Не найден элемент для исполнения: {pipelineName}", //TODO i18n
                        Trigger = string.Empty,
                        IsStartNowPipeline = isStartNowPipeline,
                    });
                    return;
                }
                              
                if (pipeline!.Blocked)
                {
                    await _journal.AddRecordAsync(new BizFlowJournalRecord()
                    {
                        Period = DateTime.Now,
                        PipelineName = pipeline.Name,
                        ItemDescription = string.Empty,
                        ItemSortOrder = 0,
                        TypeAction = TypeBizFlowJournaAction.BlockedPipeline,
                        TypeOperationId = string.Empty,
                        LaunchId = launchId,
                        Message = string.Empty,
                        Trigger = string.Empty,
                        IsStartNowPipeline = isStartNowPipeline,
                    });
                    return;
                }

                foreach (var pipelineItem in pipeline.PipelineItems.OrderBy(i => i.SortOrder))
                {
                    await _journal.AddRecordAsync(new BizFlowJournalRecord()
                    {
                        Period = DateTime.Now,
                        PipelineName = pipeline.Name,
                        ItemDescription = pipelineItem.Description,
                        ItemSortOrder = pipelineItem.SortOrder,
                        TypeAction = TypeBizFlowJournaAction.Start,
                        TypeOperationId = pipelineItem.TypeOperationId,
                        LaunchId = launchId,
                        Message = string.Empty,
                        Trigger = pipeline.CronExpression,
                        IsStartNowPipeline = isStartNowPipeline,
                    });

                    if (pipelineItem.Blocked)
                    {
                        await _journal.AddRecordAsync(new BizFlowJournalRecord()
                        {
                            Period = DateTime.Now,
                            PipelineName = pipeline.Name,
                            ItemDescription = pipelineItem.Description,
                            ItemSortOrder = pipelineItem.SortOrder,
                            TypeAction = TypeBizFlowJournaAction.BlockedPipelineItem,
                            TypeOperationId = pipelineItem.TypeOperationId,
                            LaunchId = launchId,
                            Message = string.Empty,
                            Trigger = pipeline.CronExpression,
                        });
                        continue;
                    }

                    try
                    {
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
                            workerContext.IsStartNowPipeline = isStartNowPipeline;

                            await worker.Run(workerContext);
                        }

                        await _journal.AddRecordAsync(new BizFlowJournalRecord()
                        {
                            Period = DateTime.Now,
                            PipelineName = pipeline.Name,
                            ItemDescription = pipelineItem.Description,
                            ItemSortOrder = pipelineItem.SortOrder,
                            TypeAction = TypeBizFlowJournaAction.Success,
                            TypeOperationId = pipelineItem.TypeOperationId,
                            LaunchId = launchId,
                            Message = string.Empty,
                            Trigger = pipeline.CronExpression,
                            IsStartNowPipeline = isStartNowPipeline,
                        });
                    }
                    catch (Exception)
                    {
                        await _journal.AddRecordAsync(new BizFlowJournalRecord()
                        {
                            Period = DateTime.Now,
                            PipelineName = pipeline.Name,
                            ItemDescription = pipelineItem.Description,
                            ItemSortOrder = pipelineItem.SortOrder,
                            TypeAction = TypeBizFlowJournaAction.Error,
                            TypeOperationId = pipelineItem.TypeOperationId,
                            LaunchId = launchId,
                            Message = string.Empty,
                            Trigger = pipeline.CronExpression,
                            IsStartNowPipeline = isStartNowPipeline,
                        });

                        throw;
                    }
                }
            }
            catch (Exception)
            {                
                throw;
            }
        }
    }
}
