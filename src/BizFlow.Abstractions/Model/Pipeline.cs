namespace BizFlow.Abstractions.Model
{
    /// <summary>
    /// Represents a configured pipeline consisting of a schedule and a sequence of executable items.
    /// </summary>
    public class Pipeline
    {
        /// <summary>
        /// Gets or sets the unique name of the pipeline.
        /// </summary>
        /// <value>
        /// The pipeline name. Defaults to <see cref="string.Empty"/>.
        /// </value>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the schedule that dictates when this pipeline should be executed.
        /// </summary>
        public ISchedule Schedule { get; set; }

        /// <summary>
        /// Gets or sets the collection of items (steps) that make up this pipeline.
        /// </summary>
        /// <value>
        /// A list of <see cref="PipelineItem"/> objects. Defaults to an empty list.
        /// </value>
        public List<PipelineItem> PipelineItems { get; set; } = new List<PipelineItem>();

        /// <summary>
        /// Gets or sets an optional description providing more context about the pipeline's purpose.
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the entire pipeline is currently blocked from execution.
        /// </summary>
        /// <value>
        /// <see langword="true"/> if the pipeline should be skipped; otherwise, <see langword="false"/>.
        /// </value>
        public bool Blocked { get; set; }
    }
}
