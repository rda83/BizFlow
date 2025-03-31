using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Spi;

namespace BizFlow.Core.Internal
{
    class QuartzHostedService : IHostedService, IJobsManager
    {
        private readonly IJobFactory jobFactory;

        public QuartzHostedService(IJobFactory jobFactory)
        {
            this.jobFactory = jobFactory;
        }

        public IScheduler Scheduler { get; set; }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await InitDefaultScheduler();

            // ...

            await Scheduler!.Start(cancellationToken);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await Scheduler?.Shutdown(cancellationToken);
        }

        public async Task InitDefaultScheduler()
        {
            if (Scheduler == null)
            {
                var schedulerBuilder = SchedulerBuilder.Create()
                    .UseTimeZoneConverter();

                schedulerBuilder.SchedulerId = "Scheduler-Core"; // TODO
                schedulerBuilder.InterruptJobsOnShutdown = true;
                schedulerBuilder.InterruptJobsOnShutdownWithWait = true;

                int maxBatchSize = 10; // TODO
                schedulerBuilder.MaxBatchSize = maxBatchSize <= 0 ? 10 : maxBatchSize;

                int maxThreadPoolConcurrency = 10; // TODO
                schedulerBuilder.UseDefaultThreadPool(maxConcurrency: maxThreadPoolConcurrency <= 0 ? 10 : maxThreadPoolConcurrency);

                Scheduler = await schedulerBuilder.BuildScheduler();
                Scheduler.JobFactory = jobFactory;
            }
        }

        public void CrerateJob(string name, string cronExpression)
        {

            var jobType = typeof(BizFlowJob);

            JobDataMap jobDataMap = new JobDataMap();
            jobDataMap.Add("Parameter", null);

            var job = JobBuilder
                .Create(jobType)
                .WithIdentity(name)
                .WithDescription(name)
                .SetJobData(jobDataMap)
                .Build();

            var trigger = TriggerBuilder
                .Create()
                .WithIdentity(name)
                .WithCronSchedule(cronExpression)
                .WithDescription(name)
                .Build();

            Scheduler.ScheduleJob(job, trigger, CancellationToken.None);
        }
    }
}
