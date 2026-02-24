using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
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
            var (columns, values) = BuildInsertParameters();

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

        public async Task<TEntity> GetByColumnAsync(string fieldName, object value, CancellationToken ct = default)
        {
            var sql = $@"SELECT * FROM public.{TableName} WHERE {fieldName} = @value";

            var result = await ExecuteWithConnectionAsync(async (connection, ct) =>
            {
                await using var cmd = new NpgsqlCommand(sql, connection);
                cmd.Parameters.AddWithValue("value", value);

                await using var reader = await cmd.ExecuteReaderAsync(ct);
                await reader.ReadAsync(ct);

                if (!reader.HasRows)
                {
                    return default;
                }

                return MapToEntity(reader);
            }, ct);


            return result;
        }

        public async Task<IEnumerable<TEntity>> GetPagedAsync(long lastId, int limit = 100, CancellationToken ct = default)
        {
            var sql = $@"
                SELECT * 
                FROM public.{TableName} WHERE id > @lastId
                ORDER BY id
                LIMIT @limit";

            var connection = await _uow!.GetConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("lastId", lastId);
            cmd.Parameters.AddWithValue("limit", limit);

            await using var reader = await cmd.ExecuteReaderAsync(ct);

            List<TEntity> result = [];

            while (await reader.ReadAsync(ct))
            {
                if (!reader.HasRows) { continue; }
                result.Add(MapToEntity(reader));
            }
            return result;
        }

        protected async Task<TEntity> ExecuteWithConnectionAsync(
            Func<NpgsqlConnection, CancellationToken, Task<TEntity>> operation,
            CancellationToken ct = default)
        {
            var connection = await _uow!.GetConnectionAsync();
            return await operation(connection, ct);
        }

        protected abstract TEntity MapToEntity(NpgsqlDataReader reader);
        protected abstract (string columns, string values) BuildInsertParameters();
        protected abstract void AddInsertParameters(NpgsqlCommand cmd, TEntity entity);
    }
}
