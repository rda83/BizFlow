
namespace BizFlow.Core.Model.ExecutionServices
{
    public class CancelOperationArgs
    {
        public string LaunchId { get; set; } = string.Empty;
        public string PipelineName { get; set; } = string.Empty;
        public string ItemDescription { get; set; } = string.Empty;
        public int ItemSortOrder { get; set; }
        public long ItemId { get; set; }
        public string TypeOperationId { get; set; } = string.Empty;
        public string Trigger { get; set; } = string.Empty;
        public bool IsStartNowPipeline { get; set; }
        public long CancellationRequestId { get; set; }
    }
}
