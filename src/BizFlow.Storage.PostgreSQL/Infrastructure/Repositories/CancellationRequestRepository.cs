using BizFlow.Storage.PostgreSQL.Entities;
using BizFlow.Storage.PostgreSQL.Helpers;
using Npgsql;
using System.Collections.Immutable;

namespace BizFlow.Storage.PostgreSQL.Infrastructure.Repositories
{
    class CancellationRequestRepository : BaseRepository<CancellationRequest>, IRepository<CancellationRequest>
    {
        public CancellationRequestRepository() : base()
        {
            Console.WriteLine("[DEBUG] CREATE - CancellationRequestRepository");
        }

        protected override string TableName => "bf_cancellation_requests";

        protected override ImmutableHashSet<string> SortableСolumns => throw new NotImplementedException();

        protected override ImmutableHashSet<string> FilterableСolumns => throw new NotImplementedException();

        protected override void AddInsertParameters(NpgsqlCommand cmd, CancellationRequest entity)
        {
            cmd.AddTextParameter("pipeline_name", entity.PipelineName);
            cmd.AddDateParameter("expiration_time", entity.ExpirationTime);
            cmd.AddTextParameter("description", entity.Description);
            cmd.AddBooleanParameter("closing_by_expiration_time_only", entity.ClosingByExpirationTimeOnly);
            cmd.AddDateParameter("created", entity.Created);
            cmd.AddBooleanParameter("executed", entity.Executed);
            cmd.AddDateParameter("closing_time", entity.ClosingTime);
            cmd.AddBooleanParameter("closed_after_expiration_date", entity.ClosedAfterExpirationDate);
        }

        protected override void AddUpdateParameters(NpgsqlCommand cmd, CancellationRequest entity)
        {
            cmd.AddLongParameter("Id", entity.Id);
            AddInsertParameters(cmd, entity);
        }

        protected override (string columns, string values) BuildInsertParameters()
        {
            List<string> parameters = GetFieldsName();

            var columns = string.Join(", ", parameters);
            var values = string.Join(", ", parameters.Select(k => $"@{k}"));

            return (columns, values);
        }

        protected override string BuildUpdateParameters()
        {
            List<string> parameters = GetFieldsName();
            var result = string.Join(", ", parameters.Select(k => $"{k} = @{k}"));
            return result;
        }

        protected override CancellationRequest MapToEntity(NpgsqlDataReader reader)
        {
            var id = reader.GetInt64(reader.GetOrdinal("id"));
            var pipelineName = reader.GetStringOrNull("pipeline_name");
            var expirationTime = reader.GetDateTime(reader.GetOrdinal("expiration_time"));
            var description = reader.GetStringOrNull("description");
            var closingByExpirationTimeOnly = reader.GetBoolean(reader.GetOrdinal("closing_by_expiration_time_only"));
            var created = reader.GetDateTime(reader.GetOrdinal("created"));
            var executed = reader.GetBoolean(reader.GetOrdinal("executed"));
            var closingTime = reader.GetDateTime(reader.GetOrdinal("closing_time"));
            var closedAfterExpirationDate = reader.GetBoolean(reader.GetOrdinal("closed_after_expiration_date"));

            var result = new CancellationRequest()
            {
                Id = id,
                PipelineName = pipelineName,
                ExpirationTime = expirationTime,
                Description = description,
                ClosingByExpirationTimeOnly = closingByExpirationTimeOnly,
                Created = created,
                Executed = executed,
                ClosingTime = closingTime,
                ClosedAfterExpirationDate = closedAfterExpirationDate,
            };
            return result;
        }

        private List<string> GetFieldsName()
        {
            return new List<string>
            {
                "pipeline_name",
                "expiration_time",
                "description",
                "closing_by_expiration_time_only",
                "created",
                "executed",
                "closing_time",
                "closed_after_expiration_date",
            };
        }

    }
}
