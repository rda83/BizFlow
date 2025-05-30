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

        public async Task CrerateTrigger(string name, string cronExpression)
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
