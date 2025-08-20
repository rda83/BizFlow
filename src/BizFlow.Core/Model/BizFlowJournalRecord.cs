
namespace BizFlow.Core.Model
{
    public class BizFlowJournalRecord
    {
        public DateTime Period { get; set; }
        public string PipelineName { get; set; }
        public long ItemId { get; set; }
        public string ItemDescription { get; set; }
        public int ItemSortOrder { get; set; }
        public TypeBizFlowJournaAction TypeAction { get; set; }
        public string TypeOperationId { get; set; }
        public string LaunchId { get; set; }
        public string Message { get; set; }
        public string Trigger { get; set; }
        public bool IsStartNowPipeline { get; set; }
    }
}
