using BizFlow.Core.Internal.Shared;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BizFlow.Core.Internal.Jobs
{
    public class BizFlowJob : IJob
    {
        private readonly PipelineExecutor _pipelineExecutor;
        private readonly ILogger<BizFlowJob> _logger;

        public BizFlowJob(PipelineExecutor pipelineExecutor,
            ILogger<BizFlowJob> logger)
        {
            _pipelineExecutor = pipelineExecutor;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var currentlyTriggerInfo = JobExecutionContextHelper.ExtractTriggerInfo(context);

            var scheduler = context.Scheduler;
            var currentlyExecutingJobs = await scheduler.GetCurrentlyExecutingJobs();

            var isFirstTriggerStart = currentlyExecutingJobs.Count(ctx =>
                ComparePipelineNames(ctx, currentlyTriggerInfo.PipelineName)) == 1;

            if (isFirstTriggerStart)
            {
                _logger.LogInformation($"Pipeline execution started. [PipelineName: {currentlyTriggerInfo.PipelineName}]");
                
                await _pipelineExecutor.Execute(context);

                _logger.LogInformation($"Pipeline execution completed. [PipelineName: {currentlyTriggerInfo.PipelineName}");
            }
            else
            {
                _logger.LogInformation($"Pipeline execution skipped. [PipelineName: {currentlyTriggerInfo.PipelineName}, Reason: Already running]");
            }
        }

        public bool ComparePipelineNames(IJobExecutionContext currentlyExecutingCtx, string pipelineName)
        {
            var triggerInfo = JobExecutionContextHelper.ExtractTriggerInfo(currentlyExecutingCtx);
            var result = triggerInfo.PipelineName == pipelineName;
            return result;
        }
    }
}
