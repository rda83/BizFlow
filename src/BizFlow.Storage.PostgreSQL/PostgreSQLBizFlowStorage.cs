using BizFlow.Core.Contracts.Storage;
using BizFlow.Core.Model;
using BizFlow.Storage.PostgreSQL.Infrastructure;
using BizFlow.Storage.PostgreSQL.Infrastructure.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using System;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;
using static Quartz.Logging.OperationName;
using static System.Net.Mime.MediaTypeNames;

namespace BizFlow.Storage.PostgreSQL
{
    class PostgreSQLBizFlowStorage : IBizFlowStorage
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly ILogger<PostgreSQLBizFlowStorage> _logger;
        private readonly UnitOfWork _uow;

        public PostgreSQLBizFlowStorage(ConnectionFactory connectionFactory,
            ILogger<PostgreSQLBizFlowStorage> logger,
            UnitOfWork uow)
        {
            _connectionFactory = connectionFactory ??
                throw new ArgumentNullException(nameof(connectionFactory));

            _logger = logger ??
                throw new ArgumentNullException(nameof(logger));

            _uow = uow ??
                throw new ArgumentNullException(nameof(uow));
        }

        public void Dispose()
        {
            Console.WriteLine("[DEBUG] - PostgreSQLBizFlowStorage - Dispose");
        }

        public async Task AddPipelineAsync(Pipeline pipeline, CancellationToken ct = default)
        {



            var pipelineRepository = _uow.GetRepository<Entities.Pipeline>();
            var pipelineItemRepository = _uow.GetRepository<Entities.PipelineItem>();



            throw new NotImplementedException();







            //var (columns, values, parameters) = BuildInsertParametersHeader(pipeline);

            //var sql = $@"
            //    INSERT INTO public.bf_pipelines ({columns})
            //    VALUES ({values})
            //    RETURNING *";

            //// какая еще есть бизнес логика кроме инсертов в клиентском коде  общий Add - метод как в примере,  логика ретрая в ExecuteWithConnectionAsync, 

            //await ExecuteWithConnectionAsync(async (connection, ct) =>
            //{
            //    await using var transaction = await connection.BeginTransactionAsync();
            //    try
            //    {

            //        await using var cmd = new NpgsqlCommand(sql, connection, transaction);
            //        AddInsertParametersHeader(cmd, pipeline);

            //        var id = (long) await cmd.ExecuteScalarAsync();

            //        foreach (var item in pipeline.PipelineItems)
            //        {
            //            var (columns, values, parameters) = BuildInsertParametersItem(item, id);

            //            var sql_items = $@"
            //                INSERT INTO public.bf_pipeline_items ({columns})
            //                VALUES ({values})
            //                RETURNING *";

            //            await using var cmd_item = new NpgsqlCommand(sql_items, connection, transaction);
            //            AddInsertParametersItem(cmd_item, item, id);

            //            await cmd_item.ExecuteNonQueryAsync();
            //        }

            //        await transaction.CommitAsync(ct);
            //    }
            //    catch (Exception)
            //    {
            //        await transaction.RollbackAsync(ct);
            //        throw;
            //    }
            //}, ct);
        }


        #region Headers

        private (string columns, string values, IEnumerable<string> paramNames) 
            BuildInsertParametersHeader(Pipeline pipeline)
        {
            var parameters = new Dictionary<string, object>
            {
                ["name"] = pipeline.Name,
                ["cron_expression"] = pipeline.CronExpression,
                ["description"] = pipeline.Description,
                ["blocked"] = pipeline.Blocked,
            };

            var columns = string.Join(", ", parameters.Keys);
            var values = string.Join(", ", parameters.Keys.Select(k => $"@{k.Replace("\"", "").ToLower()}"));

            return (columns, values, parameters.Keys);
        }

        private void AddInsertParametersHeader(NpgsqlCommand cmd, Pipeline pipeline)
        {
            cmd.Parameters.AddWithValue("name", pipeline.Name);
            cmd.Parameters.AddWithValue("cron_expression", pipeline.CronExpression);
            cmd.Parameters.AddWithValue("description", pipeline.Description);
            cmd.Parameters.AddWithValue("blocked", pipeline.Blocked);
        }

        #endregion


        #region Items


        private (string columns, string values, IEnumerable<string> paramNames)
            BuildInsertParametersItem(PipelineItem pipelineItem, long pipelineId)
        {
            var parameters = new Dictionary<string, object>
            {
                ["pipeline_id"] = pipelineId,
                ["type_operation_id"] = pipelineItem.TypeOperationId ?? string.Empty,
                ["sort_order"] = pipelineItem.SortOrder,
                ["description"] = pipelineItem.Description ?? string.Empty,
                ["blocked"] = pipelineItem.Blocked,
                ["options"] = pipelineItem.Options,
            };

            var columns = string.Join(", ", parameters.Keys);
            var values = string.Join(", ", parameters.Keys.Select(k => $"@{k.Replace("\"", "").ToLower()}"));

            return (columns, values, parameters.Keys);
        }

        private void AddInsertParametersItem(NpgsqlCommand cmd, PipelineItem pipelineItem, long pipelineId)
        {
            cmd.Parameters.AddWithValue("pipeline_id", pipelineId);
            cmd.Parameters.AddWithValue("type_operation_id", pipelineItem.TypeOperationId ?? string.Empty);
            cmd.Parameters.AddWithValue("sort_order", pipelineItem.SortOrder);
            cmd.Parameters.AddWithValue("description", pipelineItem.Description ?? string.Empty);
            cmd.Parameters.AddWithValue("blocked", pipelineItem.Blocked);
            cmd.Parameters.AddWithValue("options", pipelineItem.Options);
        }



        #endregion

        private async Task ExecuteWithConnectionAsync(
            Func<NpgsqlConnection, CancellationToken, Task> operation,
            CancellationToken ct = default)
        {
            await using var connection = await _connectionFactory.CreateConnectionAsync(ct);
            await operation(connection, ct);
        }
    }
}
