using System.Text.Json;

namespace BizFlow.Abstractions.Features.AddPipeline
{
    public class AddPipelineCommand
    {
        public string Name { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool Blocked { get; set; }
        public List<AddPipelineItemCommand> PipelineItems { get; set; } = [];
    }

    public class AddPipelineItemCommand
    {
        public string? TypeOperationId { get; set; }
        public int SortOrder { get; set; }
        public string? Description { get; set; }
        public bool Blocked { get; set; }
        public JsonElement Options { get; set; }
    }
}
