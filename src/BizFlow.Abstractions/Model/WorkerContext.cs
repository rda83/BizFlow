using System.Text.Json;

namespace BizFlow.Abstractions.Model
{
    /// <summary>
    /// Represents the context passed to an <see cref="IWorker"/> during pipeline execution.
    /// Contains metadata about the current run and the specific operation to be performed.
    /// </summary>
    public class WorkerContext
    {
        /// <summary>
        /// Gets or sets the unique identifier for the current pipeline execution instance (launch).
        /// </summary>
        public string LaunchId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the type identifier of the operation to be executed by the worker.
        /// </summary>
        public string TypeOperationId { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the pipeline currently being executed.
        /// </summary>
        public string PipelineName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the JSON options associated with the specific pipeline item.
        /// </summary>
        public JsonElement Options { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the pipeline was manually triggered to start immediately,
        /// rather than executed on its regular schedule.
        /// </summary>
        public bool IsStartNowPipeline { get; set; }
    }
}
