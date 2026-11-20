namespace BizFlow.Abstractions.Model
{
    /// <summary>
    /// Represents a single event record in the pipeline execution journal.
    /// </summary>
    /// <remarks>
    /// This model captures the lifecycle events of pipelines and their items, 
    /// such as starts, successes, errors, cancellations, and blocked states.
    /// It is designed to be easily persisted to a database or logged as structured data.
    /// </remarks>
    public class JournalRecord
    {
        /// <summary>
        /// Gets or sets the date and time when the event occurred.
        /// </summary>
        public DateTime Period { get; set; }

        /// <summary>
        /// Gets or sets the name of the pipeline associated with this event.
        /// </summary>
        public string PipelineName { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier of the specific pipeline item (step).
        /// </summary>
        /// <remarks>
        /// May be <c>0</c> if the event relates to the pipeline as a whole rather than a specific item.
        /// </remarks>
        public long ItemId { get; set; }

        /// <summary>
        /// Gets or sets the description of the pipeline item.
        /// </summary>
        public string ItemDescription { get; set; }

        /// <summary>
        /// Gets or sets the sort order of the pipeline item within the pipeline.
        /// </summary>
        public int ItemSortOrder { get; set; }

        /// <summary>
        /// Gets or sets the type of action recorded in this journal entry.
        /// </summary>
        public TypeBizFlowJournalAction TypeAction { get; set; }

        /// <summary>
        /// Gets or sets the operation type identifier used to resolve the worker.
        /// </summary>
        public string TypeOperationId { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the pipeline execution instance (launch).
        /// </summary>
        public string LaunchId { get; set; }

        /// <summary>
        /// Gets or sets additional details or error messages associated with the event.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the trigger that initiated the pipeline execution.
        /// </summary>
        public string Trigger { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the pipeline was manually started immediately,
        /// rather than executed on its regular schedule.
        /// </summary>
        public bool IsStartNow { get; set; }
    }
}
