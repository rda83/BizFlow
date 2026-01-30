
using Npgsql;

namespace BizFlow.Storage.PostgreSQL.Infrastructure
{
    public class ConnectionFactory : IAsyncDisposable 
    {
        public NpgsqlDataSource DataSource { get; }


        public ConnectionFactory(string connectionString)
        {
            var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
            DataSource = dataSourceBuilder.Build();
        }


        public async Task<NpgsqlConnection> CreateConnectionAsync(CancellationToken ct = default)
        {
            var connection = await DataSource.OpenConnectionAsync(ct);
            return connection;
        }

        public async ValueTask DisposeAsync()
        {
            await DataSource.DisposeAsync();
        }
    }
}
