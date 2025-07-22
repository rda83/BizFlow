using BizFlow.Core.Internal.Shared;
using Quartz;

namespace BizFlow.Core.Internal.Jobs
{
    [DisallowConcurrentExecution] //возможно ли сделать два разных джоба?
    public class BizFlowJob : IJob
    {
        private readonly PipelineExecutor _pipelineExecutor;
  
        public BizFlowJob(PipelineExecutor pipelineExecutor)
        {
            _pipelineExecutor = pipelineExecutor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await _pipelineExecutor.Execute(context);

        }
    }
}
