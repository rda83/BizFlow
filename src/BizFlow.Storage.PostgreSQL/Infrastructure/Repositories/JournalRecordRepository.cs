using BizFlow.Storage.PostgreSQL.Entities;
using BizFlow.Storage.PostgreSQL.Helpers;
using Npgsql;


namespace BizFlow.Storage.PostgreSQL.Infrastructure.Repositories
{
    class JournalRecordRepository : BaseRepository<JournalRecord>, IRepository<JournalRecord>
    {
        public JournalRecordRepository() : base()
        {
            Console.WriteLine("[DEBUG] CREATE - JournalRecordRepository");
        }

        protected override string TableName => "bf_journal";

        protected override void AddInsertParameters(NpgsqlCommand cmd, JournalRecord entity)
        {
            cmd.AddTimestampParameter("period", entity.Period);
            cmd.AddTextParameter("pipeline_name", entity.PipelineName);
            cmd.AddLongParameter("item_id", entity.ItemId);
            cmd.AddTextParameter("item_description", entity.ItemDescription);
            cmd.AddIntParameter("item_sort_order", entity.ItemSortOrder);
            cmd.AddTextParameter("type_action", entity.TypeAction.ToString());
            cmd.AddTextParameter("type_operation_id", entity.TypeOperationId);
            cmd.AddTextParameter("launch_id", entity.LaunchId);
            cmd.AddTextParameter("message", entity.Message);
            cmd.AddTextParameter("trigger", entity.Trigger);
            cmd.AddBooleanParameter("is_start_now", entity.IsStartNow);
        }

        protected override void AddUpdateParameters(NpgsqlCommand cmd, JournalRecord entity)
        {
            throw new NotImplementedException();
        }

        protected override (string columns, string values) BuildInsertParameters()
        {
            var parameters = new List<string>
            {
                "period",
                "pipeline_name",
                "item_id",
                "item_description",
                "item_sort_order",
                "type_action",
                "type_operation_id",
                "launch_id",
                "message",
                "trigger",
                "is_start_now"
            };

            var columns = string.Join(", ", parameters);
            var values = string.Join(", ", parameters.Select(k => $"@{k}"));

            return (columns, values);
        }

        protected override string BuildUpdateParameters()
        {
            throw new NotImplementedException();
        }

        protected override JournalRecord MapToEntity(NpgsqlDataReader reader)
        {
            var id = reader.GetInt64(reader.GetOrdinal("id"));
            var period = reader.GetDateTime(reader.GetOrdinal("period"));
            var pipelineName = reader.GetStringOrNull("pipeline_name");
            var itemId = reader.GetInt64(reader.GetOrdinal("item_id"));
            var itemDescription = reader.GetStringOrNull("item_description");
            var itemSortOrder = reader.GetInt32(reader.GetOrdinal("item_sort_order"));
            var typeAction = reader.GetEnumValue<Core.Model.TypeBizFlowJournalAction>("type_action");   
            var typeOperationId = reader.GetStringOrNull("type_operation_id");
            var launchId = reader.GetStringOrNull("launch_id");
            var message = reader.GetStringOrNull("message");
            var trigger = reader.GetStringOrNull("trigger");
            var isStartNow = reader.GetBoolean(reader.GetOrdinal("is_start_now"));

            var result = new JournalRecord()
            {
                Id = id,
                Period = period,
                PipelineName = pipelineName,
                ItemId = itemId,
                ItemDescription = itemDescription,
                ItemSortOrder = itemSortOrder,
                TypeAction = typeAction.Value,
                TypeOperationId = typeOperationId,
                LaunchId = launchId,
                Message = message,
                Trigger = trigger,
                IsStartNow = isStartNow,    
            };
            return result;
        }
    }
}
