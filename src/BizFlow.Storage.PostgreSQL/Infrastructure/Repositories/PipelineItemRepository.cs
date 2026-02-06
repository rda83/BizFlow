using BizFlow.Storage.PostgreSQL.Entities;
using BizFlow.Storage.PostgreSQL.Helpers;
using Npgsql;
using System.Text.Json;

namespace BizFlow.Storage.PostgreSQL.Infrastructure.Repositories
{
    class PipelineItemRepository : BaseRepository<PipelineItem>, IRepository<PipelineItem>
    {
        public PipelineItemRepository() : base()
        {
            Console.WriteLine("[DEBUG] CREATE - PipelineItemRepository");
        }

        protected override string TableName => "bf_pipeline_items";

        protected override void AddInsertParameters(NpgsqlCommand cmd, PipelineItem entity)
        {
            cmd.Parameters.AddWithValue("pipeline_id", entity.PipelineId);
            cmd.Parameters.AddWithValue("type_operation_id", entity.TypeOperationId ?? string.Empty);
            cmd.Parameters.AddWithValue("sort_order", entity.SortOrder);
            cmd.Parameters.AddWithValue("description", entity.Description ?? string.Empty);
            cmd.Parameters.AddWithValue("blocked", entity.Blocked);
            cmd.Parameters.AddWithValue("options", entity.Options);
        }

        protected override (string columns, string values) BuildInsertParameters()
        {
            var parameters = new List<string>
            {
                "pipeline_id",
                "type_operation_id",
                "sort_order",
                "description",
                "blocked",
                "options",
            };

            var columns = string.Join(", ", parameters);
            var values = string.Join(", ", parameters.Select(k => $"@{k}"));

            return (columns, values);
        }

        protected override PipelineItem MapToEntity(NpgsqlDataReader reader)
        {
            var id = reader.GetInt64(reader.GetOrdinal("id"));
            var pipelineId = reader.GetInt64(reader.GetOrdinal("pipeline_id"));
            var typeOperationId = reader.GetStringOrNull("type_operation_id");
            var sortOrder = reader.GetInt32(reader.GetOrdinal("sort_order"));
            var description = reader.GetStringOrNull("description");
            var blocked = reader.GetBoolean(reader.GetOrdinal("blocked"));
            var options = reader.GetStringOrNull("options");
            var createdAt = reader.GetDateTime(reader.GetOrdinal("created_at"));
            var updatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"));

            var result = new PipelineItem()
            {
                Id = id,
                PipelineId = pipelineId,
                TypeOperationId = typeOperationId,
                SortOrder = sortOrder,
                Description = description,
                Blocked = blocked,
                //Options = options,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
            };

            return result;
        }
    }
}
