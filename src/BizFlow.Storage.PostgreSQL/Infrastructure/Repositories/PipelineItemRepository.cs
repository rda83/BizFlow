using BizFlow.Storage.PostgreSQL.Entities;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace BizFlow.Storage.PostgreSQL.Infrastructure.Repositories
{
    class PipelineItemRepository : BaseRepository<PipelineItem>
    {
        public PipelineItemRepository(ConnectionFactory connectionFactory, ILogger<BaseRepository<PipelineItem>> logger) : base(connectionFactory, logger)
        {
        }

        protected override string TableName => "bf_pipeline_items";

        protected override void AddInsertParameters(NpgsqlCommand cmd, PipelineItem entity)
        {
            throw new NotImplementedException();
        }

        protected override (string columns, string values, IEnumerable<string> paramNames) BuildInsertParameters(PipelineItem entity)
        {
            throw new NotImplementedException();
        }

        protected override PipelineItem MapToEntity(NpgsqlDataReader reader)
        {
            throw new NotImplementedException();
        }
    }
}
