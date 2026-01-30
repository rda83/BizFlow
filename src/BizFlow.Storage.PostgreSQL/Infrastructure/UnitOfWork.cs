
using BizFlow.Storage.PostgreSQL.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace BizFlow.Storage.PostgreSQL.Infrastructure
{
    class UnitOfWork : IAsyncDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ConnectionFactory _connectionFactory;
        private readonly ILogger<UnitOfWork> _logger;


        private NpgsqlConnection? _connection;
        private NpgsqlTransaction? _transaction;

        public UnitOfWork(IServiceProvider serviceProvider, ConnectionFactory connectionFactory,
             ILogger<UnitOfWork> logger)
        {
            _connectionFactory = connectionFactory ??
                throw new ArgumentNullException(nameof(connectionFactory));

            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));

            _serviceProvider = serviceProvider ??
                throw new ArgumentNullException(nameof(serviceProvider));

            Console.WriteLine("[DEBUG] UnitOfWork - Constructor");
        }

        public async Task<NpgsqlConnection> GetConnectionAsync(CancellationToken ct = default)
        {
            if (_connection == null)
            {
                _connection = await _connectionFactory.CreateConnectionAsync(ct);
            }
            return _connection;
        }

        public async Task BeginTransactionAsync(CancellationToken ct = default)
        {
            if (_transaction !=null)
            {
                throw new InvalidOperationException("Transaction already started");
            }

            var connection = await GetConnectionAsync(ct);
            _transaction = await connection.BeginTransactionAsync(ct);
        }

        public async Task CommitAsync(CancellationToken ct = default)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction to commit");
            }

            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackAsync(CancellationToken ct = default)
        {
            if (_transaction == null)
            {
                throw new InvalidOperationException("No transaction to commit");
            }

            await _transaction.RollbackAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            var repository = _serviceProvider.GetRequiredService<IRepository<TEntity>>();
            repository.SetUnitOfWork(this);
            return repository;
        }

        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
            }

            if (_connection != null)
            {
                await _connection.DisposeAsync();
            }

            Console.WriteLine("[DEBUG] UnitOfWork - DisposeAsync");          
        }
    }
}
