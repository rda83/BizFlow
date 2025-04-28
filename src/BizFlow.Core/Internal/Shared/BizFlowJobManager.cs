using BizFlow.Core.Contracts;
using Quartz;

namespace BizFlow.Core.Internal.Shared
{
    public class BizFlowJobManager
    {
        private readonly ISchedulerFactory schedulerFactory;
        private readonly IPipelineService pipelineService;


        public BizFlowJobManager(ISchedulerFactory schedulerFactory, IPipelineService pipelineService)
        {
            this.schedulerFactory = schedulerFactory;
            this.pipelineService = pipelineService;
        }

        //public async Task CrerateJob(string name, string cronExpression)
        //{
        //    var scheduler = await _schedulerFactory.GetScheduler();

        //    var jobType = typeof(BizFlowJob);

        //    JobDataMap jobDataMap = new JobDataMap();
        //    jobDataMap.Add("Parameter", null);

        //    var job = JobBuilder
        //        .Create(jobType)
        //        .WithIdentity(name)
        //        .WithDescription(name)
        //        .SetJobData(jobDataMap)
        //        .Build();

        //    var trigger = TriggerBuilder
        //        .Create()
        //        .WithIdentity(name)
        //        .WithCronSchedule(cronExpression)
        //        .WithDescription(name)
        //        .Build();

        //    await scheduler.ScheduleJob(job, trigger, CancellationToken.None);

        //}

        public async Task CrerateTrigger(string name, string cronExpression) //Теперь мы не создаем на каждый пайплайн свой джоб, он один. теперь триггер == пайплайн. 
        {
            var scheduler = await schedulerFactory.GetScheduler();

            var trigger = TriggerBuilder.Create()
                .ForJob("bizFlowDefaultJob")
                .WithIdentity(name)
                .WithCronSchedule(cronExpression)
                .Build();

            await scheduler.ScheduleJob(trigger);
        }
    }
}
