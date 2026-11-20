using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BizFlow.Abstractions.Model
{

    /// <summary>
    /// Defines the types of actions and lifecycle events recorded in the execution journal.
    /// </summary>
    /// <remarks>
    /// This enumeration is configured to be serialized as strings in JSON 
    /// (e.g., <c>"start"</c>, <c>"error"</c>) using <see cref="System.Text.Json.Serialization.JsonStringEnumConverter"/>.
    /// </remarks>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TypeBizFlowJournalAction
    {

        /// <summary>
        /// Indicates that a pipeline item has started executing.
        /// </summary>
        [EnumMember(Value = "start")]
        Start,

        /// <summary>
        /// Indicates that a pipeline item executed successfully.
        /// </summary>
        [EnumMember(Value = "success")]
        Success,

        /// <summary>
        /// Indicates that an entire pipeline was skipped because it is marked as blocked.
        /// </summary>
        [EnumMember(Value = "blocked_pipeline")]
        BlockedPipeline,

        /// <summary>
        /// Indicates that a specific pipeline item was skipped because it is marked as blocked.
        /// </summary>
        [EnumMember(Value = "blocked_pipeline_item")]
        BlockedPipelineItem,

        /// <summary>
        /// Indicates that an error occurred during execution.
        /// </summary>
        [EnumMember(Value = "error")]
        Error,

        /// <summary>
        /// Indicates an informational event or a general note.
        /// </summary>
        [EnumMember(Value = "info")]
        Info,

        /// <summary>
        /// Indicates that a pipeline or a pipeline item was cancelled during execution.
        /// </summary>
        [EnumMember(Value = "canceled")]
        Cancelled
    }
}
