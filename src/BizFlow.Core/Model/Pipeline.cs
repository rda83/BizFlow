namespace BizFlow.Core.Model
{
    public class Pipeline
    {
        public string Name { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;

        public List<PipelineItem> PipelineItems = new List<PipelineItem>();
        public string Description { get; set; }
        public bool Blocked { get; set; }
    }
}
