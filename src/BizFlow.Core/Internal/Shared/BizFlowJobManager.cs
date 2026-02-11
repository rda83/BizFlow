using Microsoft.AspNetCore.Mvc;
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
                ?? throw new InvalidOperationException($"TriggerKey with name '{pipelineName}' not found.");

            await scheduler.UnscheduleJob(triggerKey, cancellationToken);
        }

        public async Task StartNow(string pipelineName, string launchId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(pipelineName))
            {
                throw new ArgumentNullException(
                    nameof(pipelineName),
                    "Pipeline name cannot be null, empty, or consist only of white-space characters."
                );
            }

            if (string.IsNullOrWhiteSpace(launchId))
            {
                throw new ArgumentNullException(
                    nameof(launchId),
                    "Run identifier cannot be null, empty, or consist only of white-space characters."
                );
            }

            var triggerName = $"StartNowTrigger-{pipelineName}";
            var isTriggerExists = await TriggerCheckExists(triggerName, cancellationToken);
            if (isTriggerExists)
            {
                throw new Exception($"A trigger with the key: {triggerName} already exists.");
            }

            var trigger = TriggerBuilder
                .Create()
                .ForJob("bizFlowDefaultJob")
                .WithIdentity(triggerName)
                .UsingJobData("launchId", launchId)
                .UsingJobData("pipelineName", pipelineName)
                .StartNow()
                .Build();

            var scheduler = await schedulerFactory.GetScheduler(cancellationToken);
            await scheduler.ScheduleJob(trigger, cancellationToken);
        }

        public async Task<bool> TriggerCheckExists(string triggerName,
            CancellationToken cancellationToken = default)
        {
            var scheduler = await schedulerFactory.GetScheduler(cancellationToken);

            var triggerKey = new TriggerKey(triggerName);
            var result = await scheduler.CheckExists(triggerKey);
            return result;
        }
    }
}
