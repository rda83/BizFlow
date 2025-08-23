
namespace BizFlow.Core.Model
{
    public class StatusPipelineResult
    {
        public string PipelineName { get; set; }
        public string Description { get; set; }
        public bool Complete { get; set; }
        public bool Success { get; set; }
        public List<StatusPipelineResultItem> Items { get; set; }
        public string CronExpression { get; set; } = string.Empty;
        public bool IsStartNowPipeline { get; set; }
        public StatusPipelineResult()
        {
            Items = new List<StatusPipelineResultItem>();
        }
    }

    public class StatusPipelineResultItem
    {
        public string PipelineItemDescription { get; set; }
        public long SortOrder { get; set; }
        public string? TypeOperationId { get; set; }
        public DateTime? Started { get; set; }
        public DateTime? Finished { get; set; }
        public string? LastOperationAction { get; set; }
        public List<string> Messages { get; set; }
        public bool Complete { get; set; }
        public bool Success { get; set; }

        public StatusPipelineResultItem()
        {
            Messages = new List<string>();
        }
    }
}
