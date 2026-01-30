using Microsoft.Extensions.Logging;
using Npgsql;

namespace BizFlow.Storage.PostgreSQL.Infrastructure.Repositories
{
    abstract class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private UnitOfWork? _uow;

        protected abstract string TableName { get; }

        public BaseRepository() { }

        public void SetUnitOfWork(UnitOfWork uow)
        {
            _uow = uow ??
                throw new ArgumentNullException(nameof(uow));
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

            throw new NotImplementedException();

            //await using var connection = await _connectionFactory.CreateConnectionAsync(ct);
            //return await operation(connection, ct);
        }

        protected abstract TEntity MapToEntity(NpgsqlDataReader reader);
        protected abstract (string columns, string values, IEnumerable<string> paramNames)
            BuildInsertParameters(TEntity entity);
        protected abstract void AddInsertParameters(NpgsqlCommand cmd, TEntity entity);


    }
}
