using Microsoft.Extensions.Logging;
using Quartz;
using Quartz.Impl.AdoJobStore.Common;

namespace BizFlow.Core.Internal
{
    public class BizFlowJob : IJob
    {
        private readonly ILogger<BizFlowJob> logger;

        public BizFlowJob(ILogger<BizFlowJob> logger)
        {
            this.logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            logger.LogInformation($"{context.JobDetail.Description} SampleJob executed at {DateTime.Now}");
            return Task.CompletedTask;

            //if (_isFirstStart)
            //{
            //    Task.Delay(60000).GetAwaiter().GetResult();
            //    _isFirstStart = false;
            //}

            //using (var scope = _provider.CreateScope())
            //{
            //    var routineOperationRunner = scope.ServiceProvider.GetService<IRoutineOperationRunner>();

            //    var tc = new TriggerContext();
            //    tc.OperationName = context.JobDetail.Key.Name;

            //    if (context.Trigger is Quartz.Impl.Triggers.CronTriggerImpl)
            //    {
            //        if (!routineOpsServiceEnabled)
            //            return Task.CompletedTask;

            //        tc.LaunchId = Guid.NewGuid().ToString();
            //        tc.RunUnblockedOnlyOps = RUN_UNBLOCKED_ONLY_OPS;

            //        var triggerName = ((Quartz.Impl.Triggers.AbstractTrigger)context.Trigger).Name;
            //        tc.TriggerName = triggerName;

            //        var cronExpression = ((Quartz.Impl.Triggers.CronTriggerImpl)context.Trigger).CronExpressionString!;
            //        tc.CronExpression = cronExpression;
            //    }
            //    else if (context.Trigger is Quartz.Impl.Triggers.SimpleTriggerImpl)
            //    {
            //        var trigger = ((Quartz.Impl.Triggers.SimpleTriggerImpl)context.Trigger);
            //        tc.TriggerName = trigger.Name;

            //        tc.LaunchId = trigger.JobDataMap.GetString("launchId");
            //        tc.CronExpression = trigger.JobDataMap.GetString("forTrigger");
            //        tc.RunUnblockedOnlyOps = trigger.JobDataMap.GetBoolean("runUnblockedOnlyOps");
            //    }
            //    else
            //    {
            //        throw new ArgumentException($"Unknown trigger type. [{context.Trigger.GetType()}]");
            //    }
            //    routineOperationRunner!.Run(tc);
            //}
            //return Task.CompletedTask;

        }
    }
}
