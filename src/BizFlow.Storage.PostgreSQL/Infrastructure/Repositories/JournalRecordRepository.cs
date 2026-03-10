//using BizFlow.Core.Model;
//using BizFlow.Core.Model;
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



            //period
            //pipeline_name
            //item_description
            //item_sort_order
            //type_action
            //type_operation_id
            //launch_id
            //message
            //trigger
            //is_start_now
            //item_id




            throw new NotImplementedException();
        }

        protected override (string columns, string values) BuildInsertParameters()
        {
            //new NpgsqlParameter("@status", entity.Status.ToString()));  // ToString()

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
