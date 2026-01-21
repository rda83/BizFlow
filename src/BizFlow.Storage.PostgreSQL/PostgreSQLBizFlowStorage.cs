using BizFlow.Core.Contracts.Storage;
using BizFlow.Core.Model;
using BizFlow.Storage.PostgreSQL.Infrastructure;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;

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

        public async Task AddPipelineAsync(Pipeline pipeline, CancellationToken ct = default)
        {
            var (columns, values, parameters) = BuildInsertParameters(pipeline);

            var sql = $@"
                INSERT INTO public.""Pipelines"" ({columns})
                VALUES ({values})
                RETURNING *";



            // Транзакция, два инсерта, логика ретрая в ExecuteWithConnectionAsync, общий Add - метод как в примере
            // создание таблиц bf_pipelines, bf_pipeline_items
            

            await ExecuteWithConnectionAsync(async (connection, ct) =>
            {
                await using var cmd = new NpgsqlCommand(sql, connection);
                AddInsertParameters(cmd, pipeline);


                await using var reader = await cmd.ExecuteReaderAsync(ct);

                var x = await reader.ReadAsync(ct);


                var i = 0;

            }, ct);


            //throw new NotImplementedException();
        }

        private (string columns, string values, IEnumerable<string> paramNames) 
            BuildInsertParameters(Pipeline pipeline)
        {
            var parameters = new Dictionary<string, object>
            {
                ["\"Name\""] = pipeline.Name,
                ["\"CronExpression\""] = pipeline.CronExpression,
                ["\"Description\""] = pipeline.Description,
                ["\"Blocked\""] = pipeline.Blocked,
            };

            var columns = string.Join(", ", parameters.Keys);
            var values = string.Join(", ", parameters.Keys.Select(k => $"@{k.Replace("\"", "").ToLower()}"));

            return (columns, values, parameters.Keys);
        }

        private void AddInsertParameters(NpgsqlCommand cmd, Pipeline pipeline)
        {
            cmd.Parameters.AddWithValue("Name".Replace("\"", "").ToLower(), pipeline.Name + "NEW");
            cmd.Parameters.AddWithValue("CronExpression".Replace("\"", "").ToLower(), pipeline.CronExpression);
            cmd.Parameters.AddWithValue("Description".Replace("\"", "").ToLower(), pipeline.Description);
            cmd.Parameters.AddWithValue("Blocked".Replace("\"", "").ToLower(), pipeline.Blocked);
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
