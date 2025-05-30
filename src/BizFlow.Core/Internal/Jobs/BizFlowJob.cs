using BizFlow.Core.Internal.Shared;
using Quartz;

namespace BizFlow.Core.Internal.Jobs
{
    [DisallowConcurrentExecution] //возможно ли сделать два разных джоба?
    public class BizFlowJob : IJob
    {
        private readonly PipelineExecutor _pipelineExecutor;
  
        private bool _isFirstStart = true;
        public BizFlowJob(PipelineExecutor pipelineExecutor)
        {
            _pipelineExecutor = pipelineExecutor;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            //TODO Код был для singletone
            if (_isFirstStart)
            {
                Task.Delay(60000).GetAwaiter().GetResult();
                _isFirstStart = false;
            }

            await _pipelineExecutor.Execute(context);

        }
    }
}
