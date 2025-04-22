using Quartz;

namespace BizFlow.Core.Internal.Shared
{
    class BizFlowJobManager
    {
        private readonly ISchedulerFactory _schedulerFactory;

        public BizFlowJobManager(ISchedulerFactory schedulerFactory)
        {
            _schedulerFactory = schedulerFactory;
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

        public async Task CrerateJob(string name, string cronExpression) //Теперь мы не создаем на каждый пайплайн свой джоб, он один. теперь триггер == пайплайн. 
        {
            var scheduler = await _schedulerFactory.GetScheduler();

            var trigger = TriggerBuilder.Create()
                .ForJob("bizFlowDefaultJob")
                .WithIdentity(name)
                .WithCronSchedule(cronExpression)
                .Build();

            await scheduler.ScheduleJob(trigger);
        }
    }
}
