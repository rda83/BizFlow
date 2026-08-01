using BizFlow.Abstractions;
using BizFlow.Abstractions.Model;

namespace BizFlow.ExecutionJournal.Database
{
    public class ExecutionJournalDatabase : IExecutionJournal
    {
        public Task AddBlockedPipelineAsync(string launchId, bool isStartNowPipeline, string pipelineName)
        {
            throw new NotImplementedException();
        }

        public Task AddBlockedPipelineItemAsync(string launchId, Pipeline pipeline, PipelineItem pipelineItem)
        {
            throw new NotImplementedException();
        }

        public Task AddCancelledAsync(CancelOperationArgs args)
        {
            throw new NotImplementedException();
        }

        public Task AddErrorAsync(string launchId, bool isStartNowPipeline, string msg)
        {
            throw new NotImplementedException();
        }

        public Task AddErrorAsync(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem)
        {
            throw new NotImplementedException();
        }

        public Task AddStartAsync(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem)
        {
            throw new NotImplementedException();
        }

        public Task AddSuccessAsync(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem)
        {
            throw new NotImplementedException();
        }
    }
}
