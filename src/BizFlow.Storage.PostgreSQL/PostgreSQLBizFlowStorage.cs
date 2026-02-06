using BizFlow.Core.Contracts.Storage;
using BizFlow.Core.Model;
using BizFlow.Storage.PostgreSQL.Infrastructure;
using Microsoft.Extensions.Logging;

namespace BizFlow.Storage.PostgreSQL
{
    class PostgreSQLBizFlowStorage : IBizFlowStorage
    {
        private readonly ILogger<PostgreSQLBizFlowStorage> _logger;
        private readonly UnitOfWork _uow;

        public PostgreSQLBizFlowStorage(ILogger<PostgreSQLBizFlowStorage> logger,
            UnitOfWork uow)
        {
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
            try
            {
                await _uow.BeginTransactionAsync(ct);

                var pipelineRepository = _uow.GetRepository<Entities.Pipeline>();
                var pipelineItemRepository = _uow.GetRepository<Entities.PipelineItem>();

                var result = await pipelineRepository.AddAsync(new Entities.Pipeline(pipeline), ct);

                if (result.Id <= 0)
                {
                    throw new InvalidOperationException($"Falied to create pipeline. Invalid Id returned: {result.Id}");
                }

                foreach (var pipelineItem in pipeline.PipelineItems)
                {
                    var itemResult = await pipelineItemRepository.AddAsync(
                        new Entities.PipelineItem(result.Id, pipelineItem), ct);
                }

                await _uow.CommitAsync(ct);
            }
            catch (Exception)
            {
                await _uow.RollbackAsync(ct);
                throw;
            }
        }
    }
}
