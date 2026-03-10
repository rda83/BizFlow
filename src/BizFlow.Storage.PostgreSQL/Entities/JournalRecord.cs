
using BizFlow.Core.Model;

namespace BizFlow.Storage.PostgreSQL.Entities
{
    class JournalRecord
    {
        public long Id { get; set; }
        public DateTime Period { get; set; }
        public string PipelineName { get; set; } = string.Empty;
        public long ItemId { get; set; }
        public string ItemDescription { get; set; } = string.Empty;
        public int ItemSortOrder { get; set; }
        public TypeBizFlowJournalAction TypeAction { get; set; }
        public string TypeOperationId { get; set; } = string.Empty;
        public string LaunchId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Trigger { get; set; } = string.Empty;
        public bool IsStartNow { get; set; }
    }
}
