using System.Text.Json;

namespace BizFlow.Abstractions.Model
{
    /// <summary>
    /// Represents a single executable step (item) within a <see cref="Pipeline"/>.
    /// </summary>
    public class PipelineItem
    {
        /// <summary>
        /// Gets or sets the unique identifier of the pipeline item.
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the operation type identifier used to resolve the specific <see cref="IWorker"/> 
        /// implementation from the dependency injection container.
        /// </summary>
        public string? TypeOperationId { get; set; }

        /// <summary>
        /// Gets or sets the order in which this item should be executed relative to other items in the pipeline.
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// Gets or sets an optional description of the item's purpose.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this specific item is currently blocked from execution.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the item should be skipped; otherwise, <see langword="false"/>.
        /// </value>
        public bool Blocked { get; set; }

        /// <summary>
        /// Gets or sets the JSON configuration options passed to the worker during execution.
        /// </summary>
        public JsonElement Options { get; set; }
    }
}
