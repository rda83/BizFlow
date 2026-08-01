
using BizFlow.Abstractions.Model;

namespace BizFlow.Abstractions
{
    public interface IExecutionJournal
    {
        Task AddErrorAsync(string launchId, bool isStartNowPipeline, string msg);
        Task AddErrorAsync(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem);
        Task AddBlockedPipelineAsync(string launchId, bool isStartNowPipeline, string pipelineName);
        Task AddStartAsync(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem);
        Task AddBlockedPipelineItemAsync(string launchId, Pipeline pipeline, PipelineItem pipelineItem);
        Task AddSuccessAsync(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem);
        Task AddCancelledAsync(CancelOperationArgs args);
    }
}
