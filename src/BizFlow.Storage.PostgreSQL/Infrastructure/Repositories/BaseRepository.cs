using Npgsql;
using System.Collections.Immutable;

namespace BizFlow.Storage.PostgreSQL.Infrastructure.Repositories
{
    abstract class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private UnitOfWork? _uow;

        protected abstract string TableName { get; }
        protected abstract ImmutableHashSet<string> SortableСolumns { get; }
        protected abstract ImmutableHashSet<string> FilterableСolumns { get; }

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

        public async Task<TEntity> GetByUniqueColumnAsync(string fieldName, object value, CancellationToken ct = default)
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

        public async Task<IEnumerable<TEntity>> GetByColumnAsync(string fieldName, object value, CancellationToken ct = default)
        {
            var sql = $@"SELECT * FROM public.{TableName} WHERE {fieldName} = @value";

            var connection = await _uow!.GetConnectionAsync();
            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("value", value);
            
            await using var reader = await cmd.ExecuteReaderAsync(ct);

            List<TEntity> result = [];

            while (await reader.ReadAsync(ct))
            {
                if (!reader.HasRows) { continue; }
                result.Add(MapToEntity(reader));
            }

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

        public async Task<int> DeleteAsync(string fieldName, object value, CancellationToken ct = default)
        {
            var sql = $@"DELETE FROM public.{TableName} WHERE {fieldName} = @value";
            var connection = await _uow!.GetConnectionAsync();

            await using var cmd = new NpgsqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("value", value);

            var rowsAffected = await cmd.ExecuteNonQueryAsync();
            return rowsAffected;
        }

        public async Task<bool> UpdateAsync(TEntity entity, CancellationToken ct = default)
        {
            var updateParameters = BuildUpdateParameters();

            var sql = $@"
                UPDATE public.{TableName} 
                SET {updateParameters} WHERE Id = @Id";

            var connection = await _uow!.GetConnectionAsync();
            await using var cmd = new NpgsqlCommand(sql, connection);
            AddUpdateParameters(cmd, entity);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            await reader.ReadAsync(ct);

            return reader.RecordsAffected > 0;
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
        protected abstract string BuildUpdateParameters();
        protected abstract void AddUpdateParameters(NpgsqlCommand cmd, TEntity entity);


        public async Task<IEnumerable<TEntity>> GetPagedNewAsync(PagedQuery pagedQuery, CancellationToken ct = default)
        {
            var query = BuildPagedQuery(pagedQuery);
            
            var connection = await _uow!.GetConnectionAsync();

            await using var cmd = new NpgsqlCommand(query.Sql, connection);

            foreach (var parameter in query.Parameters)
            {
                cmd.Parameters.Add(parameter);
            }

            await using var reader = await cmd.ExecuteReaderAsync(ct);

            List<TEntity> result = [];

            while (await reader.ReadAsync(ct))
            {
                if (!reader.HasRows) { continue; }
                result.Add(MapToEntity(reader));
            }
            return result;
        }

        private (string Sql, List<NpgsqlParameter> Parameters) BuildPagedQuery(PagedQuery pagedQuery)
        {
            var parameters = new List<NpgsqlParameter>();
            var whereClauses = new List<string>();
            var paramIndex = 0;

            foreach (var item in pagedQuery.Filters)
            {
                if(!FilterableСolumns.Contains(item.Key))
                {
                    throw new NotSupportedException($"Field {item.Key} cannot be used in filter conditions." +
                        $"Supported fields: {string.Join(", ", FilterableСolumns)}");
                }

                var paramName = $"@p{paramIndex}";

                string condition = string.Empty;
                switch (item.Value.Operator)
                {
                    case FilterOperator.Eq:
                        condition = $"{item.Key} = {paramName}";
                        break;
                    case FilterOperator.Contains:
                        condition = $"{item.Key} ILIKE '%' || {paramName} || '%'";
                        break;
                    case FilterOperator.Gt:
                        condition = $"{item.Key} > {paramName}";
                        break;
                    case FilterOperator.Lt:
                        condition = $"{item.Key} < {paramName}";
                        break;
                    case FilterOperator.Startswith:
                        condition = $"{item.Key} ILIKE {paramName} || '%'";
                        break;
                    default:
                        throw new NotSupportedException($"Operator {item.Value.Operator} is not supported");
                }

                whereClauses.Add(condition);
                parameters.Add(new NpgsqlParameter(paramName, item.Value.Value));
                paramIndex++;
            }

            var orderByClauses = new List<string>();
            foreach (var item in pagedQuery.SortBy)
            {
                if (!SortableСolumns.Contains(item.Key))
                {
                    throw new NotSupportedException($"Field {item.Key} cannot be used for sorting." +
                        $"Supported fields: {string.Join(", ", SortableСolumns)}");
                }


                var direction = item.Value == SortType.Asc ? "ASC" : "DESC";
                orderByClauses.Add($"{item.Key} {direction} NULLS LAST");

            }

            var whereSql = whereClauses.Any() ? "WHERE " + string.Join(" AND ", whereClauses) : "";            
            var orderSql = orderByClauses.Any() ? "ORDER BY " + string.Join(", ", orderByClauses) : "";

            var sql = $@"
                SELECT * 
                FROM public.{TableName}
                {whereSql}
                {orderSql}
                OFFSET {(pagedQuery.Page - 1) * pagedQuery.PageSize} ROWS
                FETCH NEXT {pagedQuery.PageSize} ROWS ONLY;";

            return (sql, parameters);
        }
    }

    public class PagedQuery
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;

        public Dictionary<string, FilterCondition> Filters { get; set; } = [];
        public Dictionary<string, SortType> SortBy { get; set; } = [];
    }

    public class FilterCondition
    {
        public FilterOperator Operator { get; set; }
        public object? Value { get; set; }
    }

    public enum FilterOperator
    {
        Eq,
        Contains,
        Gt,
        Lt,
        Startswith,
    }

    public enum SortType
    {
        Asc,
        Desc,
    }
}
