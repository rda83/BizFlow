using Microsoft.Extensions.Logging;
using Quartz;

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
            logger.LogInformation("SampleJob executed at {0}", DateTime.Now);
            return Task.CompletedTask;
        }
    }
}
