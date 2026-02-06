
using BizFlow.Storage.PostgreSQL.Entities;
using BizFlow.Storage.PostgreSQL.Helpers;
using Npgsql;

namespace BizFlow.Storage.PostgreSQL.Infrastructure.Repositories
{
    class PipelineRepository : BaseRepository<Pipeline>, IRepository<Pipeline> 
    {
        public PipelineRepository() : base()
        {
            Console.WriteLine("[DEBUG] CREATE - PipelineRepository");
        }

        protected override string TableName => "bf_pipelines";

        protected override void AddInsertParameters(NpgsqlCommand cmd, Pipeline entity)
        {
            cmd.Parameters.AddWithValue("name", entity.Name);
            cmd.Parameters.AddWithValue("cron_expression", entity.CronExpression);
            cmd.Parameters.AddWithValue("description", entity.Description ?? string.Empty);
            cmd.Parameters.AddWithValue("blocked", entity.Blocked);
        }

        protected override (string columns, string values) BuildInsertParameters()
        {
            var parameters = new List<string>
            {
                "name",
                "cron_expression",
                "description",
                "blocked",
            };

            var columns = string.Join(", ", parameters);
            var values = string.Join(", ", parameters.Select(k => $"@{k}"));

            return (columns, values);
        }

        protected override Pipeline MapToEntity(NpgsqlDataReader reader)
        {
            var id = reader.GetInt64(reader.GetOrdinal("id"));
            var name = reader.GetStringOrNull("name");
            var cronExpression = reader.GetStringOrNull("cron_expression");
            var description = reader.GetStringOrNull("description");
            var blocked = reader.GetBoolean(reader.GetOrdinal("blocked"));
            var createdAt = reader.GetDateTime(reader.GetOrdinal("created_at"));
            var updatedAt = reader.GetDateTime(reader.GetOrdinal("updated_at"));
            var createdBy = reader.GetStringOrNull("created_by");
            var updatedBy = reader.GetStringOrNull("updated_by");

            var result = new Pipeline()
            {
                Id = id,
                Name = name!,
                CronExpression = cronExpression!,
                Description = description,
                Blocked = blocked,
                CreatedAt = createdAt,
                UpdatedAt = updatedAt,
                CreatedBy = createdBy,
                UpdatedBy = updatedBy,
            };
            return result;
        }
    }
}
