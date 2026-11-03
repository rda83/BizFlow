using BizFlow.Abstractions.Model;

namespace BizFlow.Abstractions
{
    /// <summary>
    /// A null-object implementation of <see cref="IExecutionJournal"/>.
    /// </summary>
    /// <remarks>
    /// This implementation ignores all journaling events and can be used in scenarios 
    /// where execution tracking is not required (e.g., in unit tests or when the feature is disabled).
    /// </remarks>
    public class NullExecutionJournal : IExecutionJournal
    {
        /// <inheritdoc/>
        public Task AddBlockedPipelineAsync(string launchId, bool isStartNowPipeline, string pipelineName)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task AddBlockedPipelineItemAsync(string launchId, Pipeline pipeline, PipelineItem pipelineItem)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task AddCancelledPipelineAsync(string launchId, bool isStartNowPipeline, Pipeline pipeline)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task AddCancelledPipelineItemAsync(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task AddErrorAsync(string launchId, bool isStartNowPipeline, string msg)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task AddErrorAsync(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem, string msg)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task AddStartAsync(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        public Task AddSuccessAsync(string launchId, bool isStartNowPipeline, Pipeline pipeline, PipelineItem pipelineItem)
        {
            return Task.CompletedTask;
        }
    }
}
