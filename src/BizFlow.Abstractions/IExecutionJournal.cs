
using BizFlow.Abstractions.Model;

namespace BizFlow.Abstractions
{
    public interface IExecutionJournal
    {
        Task AddError(string launchId, bool isStartNowPipeline, string msg);
        Task AddError(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem);
        Task AddBlockedPipeline(string launchId, bool isStartNowPipeline, string pipelineName);
        Task AddStart(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem);
        Task AddBlockedPipelineItem(string launchId, Pipeline pipeline, PipelineItem pipelineItem);
        Task AddSuccess(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem);
        Task AddCanceled(CancelOperationArgs args);

    }
}
