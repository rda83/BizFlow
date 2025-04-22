namespace BizFlow.Core.Model
{
    public class WorkerContext
    {
        public string LaunchId { get; set; }
        public string TypeOperationId { get; set; }
        public string PipelineName { get; set; }
        public PipelineItem PipelineItem { get; set; }
        public string TriggerName { get; set; }
        public string CronExpression { get; set; }
        public CancellationToken CancellationToken { get; set; }
    }
}
