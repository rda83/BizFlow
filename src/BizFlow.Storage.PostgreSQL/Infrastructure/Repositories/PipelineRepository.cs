using BizFlow.Storage.PostgreSQL.Entities;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace BizFlow.Storage.PostgreSQL.Infrastructure.Repositories
{
    class PipelineRepository : BaseRepository<Pipeline>
    {
        public PipelineRepository(ConnectionFactory connectionFactory, ILogger<BaseRepository<Pipeline>> logger) : base(connectionFactory, logger)
        {
        }

        protected override string TableName => "bf_pipelines";

        protected override void AddInsertParameters(NpgsqlCommand cmd, Pipeline entity)
        {
            throw new NotImplementedException();
        }

        protected override (string columns, string values, IEnumerable<string> paramNames) BuildInsertParameters(Pipeline entity)
        {
            throw new NotImplementedException();
        }

        protected override Pipeline MapToEntity(NpgsqlDataReader reader)
        {
            throw new NotImplementedException();
        }
    }
}
