using Quartz;
using Quartz.Impl.Matchers;

namespace BizFlow.Core.Internal.Shared
{
    public class BizFlowJobManager
    {
        private readonly ISchedulerFactory schedulerFactory;

        public BizFlowJobManager(ISchedulerFactory schedulerFactory)
        {
            this.schedulerFactory = schedulerFactory;
        }

        public async Task CrerateTrigger(string name, string cronExpression,
            CancellationToken cancellationToken = default)
        {
            var scheduler = await schedulerFactory.GetScheduler(cancellationToken);

            var trigger = TriggerBuilder.Create()
                .ForJob("bizFlowDefaultJob")
                .WithIdentity(name)
                .WithCronSchedule(cronExpression)
                .Build();

            await scheduler.ScheduleJob(trigger, cancellationToken);
        }

        public async Task DeleteTrigger(string pipelineName,
            CancellationToken cancellationToken = default)
        {
            var scheduler = await schedulerFactory.GetScheduler();

            var allTriggerKeys = await scheduler.GetTriggerKeys(
                GroupMatcher<TriggerKey>.AnyGroup(), cancellationToken);

            var triggerKey = allTriggerKeys.Where(i => i.Name == pipelineName).FirstOrDefault() 
                ?? throw new InvalidOperationException($"TriggerKey with name '{pipelineName}' not found."); // TODO:i18n

            await scheduler.UnscheduleJob(triggerKey, cancellationToken);
        }
    }
}
