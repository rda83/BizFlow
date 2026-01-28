using Microsoft.Extensions.Logging;
using Npgsql;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

namespace BizFlow.Storage.PostgreSQL.Infrastructure.Repositories
{
    abstract class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly ILogger<BaseRepository<TEntity>> _logger;

        protected abstract string TableName { get; }

        public BaseRepository(ConnectionFactory connectionFactory,
            ILogger<BaseRepository<TEntity>> logger)
        {
            _connectionFactory = connectionFactory ??
                throw new ArgumentNullException(nameof(connectionFactory));

            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
        }


        public async Task<TEntity> AddAsync(TEntity entity, CancellationToken ct = default)
        {
            var (columns, values, parameters) = BuildInsertParameters(entity);

            var sql = $@"
                INSERT INTO public.{TableName} ({columns})
                VALUES ({values})
                RETURNING *";

            var result = await ExecuteWithConnectionAsync(async (connection, ct) =>
            {
                await using var cmd = new NpgsqlCommand(sql, connection);
                AddInsertParameters(cmd, entity);

                await using var reader = await cmd.ExecuteReaderAsync(ct);
                await reader.ReadAsync(ct);

                return MapToEntity(reader);

            }, ct);


            return result;
        }




        protected async Task<TEntity> ExecuteWithConnectionAsync(
            Func<NpgsqlConnection, CancellationToken, Task<TEntity>> operation,
            CancellationToken ct = default)
        {
            await using var connection = await _connectionFactory.CreateConnectionAsync(ct);
            return await operation(connection, ct);
        }

        protected abstract TEntity MapToEntity(NpgsqlDataReader reader);
        protected abstract (string columns, string values, IEnumerable<string> paramNames)
            BuildInsertParameters(TEntity entity);
        protected abstract void AddInsertParameters(NpgsqlCommand cmd, TEntity entity);
    }
}
