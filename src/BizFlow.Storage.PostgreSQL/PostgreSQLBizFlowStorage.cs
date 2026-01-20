using BizFlow.Core.Contracts.Storage;
using BizFlow.Core.Model;
using BizFlow.Storage.PostgreSQL.Infrastructure;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;

namespace BizFlow.Storage.PostgreSQL
{
    class PostgreSQLBizFlowStorage : IBizFlowStorage
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly ILogger<PostgreSQLBizFlowStorage> _logger;

        public PostgreSQLBizFlowStorage(ConnectionFactory connectionFactory,
            ILogger<PostgreSQLBizFlowStorage> logger)
        {
            _connectionFactory = connectionFactory ??
                throw new ArgumentNullException(nameof(connectionFactory));

            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));
        }

        public string Ping()
        {
            Console.WriteLine("!!!!!!!!!!!   Pong");
            return "!!!!!!!!!!!   Pong";
        }
        public void Dispose()
        {
            Console.WriteLine("!!!!!!!!!!! DISPOSE  PostgreSQLBizFlowStorage");
        }

        public async Task AddPipelineAsync(Pipeline pipelineItem, CancellationToken ct = default)
        {
            var sql = """
                INSERT INTO public."Pipelines"
                ("Name", "CronExpression", "Description", "Blocked")
                VALUES('XXXXXXXXXXX', '0 50 10 ? * * *', 'Выполнение в заданное время', true);
                """;

            await ExecuteWithConnectionAsync(async (connection, ct) =>
            {
                await using var cmd = new NpgsqlCommand(sql, connection);
                await using var reader = await cmd.ExecuteReaderAsync(ct);

                var x = await reader.ReadAsync(ct);


                var i = 0;

            }, ct);


            //throw new NotImplementedException();
        }



        private async Task ExecuteWithConnectionAsync(
            Func<NpgsqlConnection, CancellationToken, Task> operation,
            CancellationToken ct = default)
        {
            await using var connection = await _connectionFactory.CreateConnectionAsync(ct);
            await operation(connection, ct);
        }


    }
}
