
using System.Text.Json;

namespace BizFlow.Core.Internal.Features.AddPipeline
{
    // Правила действий при ошибках (либо на всю операцию либо отдельно по элементам)
    // Тип Job-а (BizFlowJob : IJob) если их будет несколько


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
