
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

        public JournalRecord() { }

        public JournalRecord(Core.Model.JournalRecord journalRecord)
        {
            Period = journalRecord.Period;
            PipelineName = journalRecord.PipelineName;
            ItemId = journalRecord.ItemId;
            ItemDescription = journalRecord.ItemDescription;
            ItemSortOrder = journalRecord.ItemSortOrder;
            TypeAction = journalRecord.TypeAction;
            TypeOperationId = journalRecord.TypeOperationId;
            LaunchId = journalRecord.LaunchId;
            Message = journalRecord.Message;
            Trigger = journalRecord.Trigger;
            IsStartNow = journalRecord.IsStartNow;
        }

        public Core.Model.JournalRecord ToCoreModel()
        {
            var result = new Core.Model.JournalRecord()
            {
                Period = Period,
                PipelineName = PipelineName,
                ItemId = ItemId,
                ItemDescription = ItemDescription,
                ItemSortOrder = ItemSortOrder,
                TypeAction = TypeAction,
                TypeOperationId = TypeOperationId,
                LaunchId = LaunchId,
                Message = Message,
                Trigger = Trigger,
                IsStartNow = IsStartNow,
            };
            return result;
        }
    }
}
