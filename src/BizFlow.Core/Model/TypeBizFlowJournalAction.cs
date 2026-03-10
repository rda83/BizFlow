
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace BizFlow.Core.Model
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum TypeBizFlowJournalAction
    {
        [EnumMember(Value = "start")]
        Start,

        [EnumMember(Value = "success")]
        Success,

        [EnumMember(Value = "blocked_pipeline")]
        BlockedPipeline,

        [EnumMember(Value = "blocked_pipeline_item")]
        BlockedPipelineItem,

        [EnumMember(Value = "error")]
        Error,

        [EnumMember(Value = "info")]
        Info,

        [EnumMember(Value = "canceled")]
        Canceled
    }
}
