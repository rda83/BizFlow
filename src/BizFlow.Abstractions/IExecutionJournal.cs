
using BizFlow.Abstractions.Model;

namespace BizFlow.Abstractions
{
    /// <summary>
    /// Defines a contract for recording pipeline execution events.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface are responsible for persisting or logging 
    /// the lifecycle events of pipelines and their individual items, such as starts, 
    /// successes, failures, cancellations, and blocked states.
    /// </remarks>
    public interface IExecutionJournal
    {
        /// <summary>
        /// Records a general error that is not associated with a specific pipeline item.
        /// </summary>
        /// <param name="launchId">The unique identifier for the current pipeline execution instance.</param>
        /// <param name="isStartNowPipeline">Indicates whether the pipeline was triggered manually rather than by the scheduler.</param>
        /// <param name="msg">The error message detailing what went wrong.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddErrorAsync(string launchId, bool isStartNowPipeline, string msg);

        /// <summary>
        /// Records an error that occurred during the execution of a specific pipeline item.
        /// </summary>
        /// <param name="launchId">The unique identifier for the current pipeline execution instance.</param>
        /// <param name="isStartNowPipeline">Indicates whether the pipeline was triggered manually rather than by the scheduler.</param>
        /// <param name="pipeline">The pipeline being executed.</param>
        /// <param name="pipelineItem">The specific item that failed.</param>
        /// <param name="msg">The error message detailing what went wrong.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddErrorAsync(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem, string msg);

        /// <summary>
        /// Records that an entire pipeline was skipped because it is marked as blocked.
        /// </summary>
        /// <param name="launchId">The unique identifier for the current pipeline execution instance.</param>
        /// <param name="isStartNowPipeline">Indicates whether the pipeline was triggered manually rather than by the scheduler.</param>
        /// <param name="pipelineName">The name of the blocked pipeline.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddBlockedPipelineAsync(string launchId, bool isStartNowPipeline, string pipelineName);

        /// <summary>
        /// Records that a pipeline item has started executing.
        /// </summary>
        /// <param name="launchId">The unique identifier for the current pipeline execution instance.</param>
        /// <param name="isStartNowPipeline">Indicates whether the pipeline was triggered manually rather than by the scheduler.</param>
        /// <param name="pipeline">The pipeline containing the item.</param>
        /// <param name="pipelineItem">The item that is starting.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddStartAsync(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem);

        /// <summary>
        /// Records that a specific pipeline item was skipped because it is marked as blocked.
        /// </summary>
        /// <param name="launchId">The unique identifier for the current pipeline execution instance.</param>
        /// <param name="pipeline">The pipeline containing the item.</param>
        /// <param name="pipelineItem">The item that was blocked.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddBlockedPipelineItemAsync(string launchId, Pipeline pipeline, PipelineItem pipelineItem);

        /// <summary>
        /// Records that a pipeline item executed successfully.
        /// </summary>
        /// <param name="launchId">The unique identifier for the current pipeline execution instance.</param>
        /// <param name="isStartNowPipeline">Indicates whether the pipeline was triggered manually rather than by the scheduler.</param>
        /// <param name="pipeline">The pipeline containing the item.</param>
        /// <param name="pipelineItem">The item that succeeded.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddSuccessAsync(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem);

        /// <summary>
        /// Records that the entire pipeline execution was cancelled.
        /// </summary>
        /// <param name="launchId">The unique identifier for the current pipeline execution instance.</param>
        /// <param name="isStartNowPipeline">Indicates whether the pipeline was triggered manually rather than by the scheduler.</param>
        /// <param name="pipeline">The pipeline that was cancelled.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddCancelledPipelineAsync(string launchId, bool isStartNowPipeline, Pipeline pipeline);

        /// <summary>
        /// Records that a specific pipeline item was cancelled during execution.
        /// </summary>
        /// <param name="launchId">The unique identifier for the current pipeline execution instance.</param>
        /// <param name="isStartNowPipeline">Indicates whether the pipeline was triggered manually rather than by the scheduler.</param>
        /// <param name="pipeline">The pipeline containing the item.</param>
        /// <param name="pipelineItem">The item that was cancelled.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task AddCancelledPipelineItemAsync(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem);        
    }
}
