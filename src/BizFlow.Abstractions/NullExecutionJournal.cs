using BizFlow.Abstractions.Model;

namespace BizFlow.Abstractions
{
    public class NullExecutionJournal : IExecutionJournal
    {
        public Task AddBlockedPipelineAsync(string launchId, bool isStartNowPipeline, string pipelineName)
        {
            return Task.CompletedTask;
        }

        public Task AddBlockedPipelineItemAsync(string launchId, Pipeline pipeline, PipelineItem pipelineItem)
        {
            return Task.CompletedTask;
        }

        public Task AddCancelledAsync(CancelOperationArgs args)
        {
            throw new NotImplementedException();
        }

        public Task AddErrorAsync(string launchId, bool isStartNowPipeline, string msg)
        {
            return Task.CompletedTask;
        }

        public Task AddErrorAsync(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem)
        {
            return Task.CompletedTask;
        }

        public Task AddStartAsync(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem)
        {
            return Task.CompletedTask;
        }

        public Task AddSuccessAsync(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem)
        {
            return Task.CompletedTask;
        }
    }
}
