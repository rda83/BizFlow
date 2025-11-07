using Quartz;

namespace BizFlow.Core.Internal.Shared
{
    static class JobExecutionContextHelper
    {
        public static (string LaunchId, string PipelineName, bool IsStartNowPipeline) ExtractTriggerInfo(IJobExecutionContext context)
        {
            var launchId = string.Empty;
            var pipelineName = string.Empty;
            var isStartNowPipeline = false;

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

            return (launchId, pipelineName, isStartNowPipeline);
        }
    }
}
