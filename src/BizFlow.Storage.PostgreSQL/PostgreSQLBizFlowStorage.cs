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

        public async Task<bool> PipelineNameExistAsync(string pipelineName, CancellationToken ct = default)
        {
            try
            {
                var pipelineRepository = _uow.GetRepository<Entities.Pipeline>();
                var result = await pipelineRepository.GetByUniqueColumnAsync("name", pipelineName, ct);
           
                return result != null;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<(IReadOnlyCollection<Pipeline> Pipelines, long MaxId)> GetPipelinesAsync(
            long lastId, int limit = 100, CancellationToken cancellationToken = default)
        {
            long maxId = lastId;
            List<Pipeline> result = [];

            var pipelineRepository = _uow.GetRepository<Entities.Pipeline>();
            var entities = await pipelineRepository.GetPagedAsync(lastId, limit, cancellationToken);

            if (entities != null)
            {
                foreach (var item in entities)
                {
                    maxId = Math.Max(maxId, item.Id);
                    result.Add(item.ToCoreModel());
                }
            }
            return (result.AsReadOnly(), maxId);
        }

        public async Task<Pipeline?> GetPipelineAsync(string pipelineName, CancellationToken cancellationToken = default)
        {

            var pipelineRepository = _uow.GetRepository<Entities.Pipeline>();
            var pipelineItemRepository = _uow.GetRepository<Entities.PipelineItem>();

            var pipelineEntity = await pipelineRepository.GetByUniqueColumnAsync("name", pipelineName);

            if (pipelineEntity != null)
            {
                var pipelineItems = await pipelineItemRepository.GetByColumnAsync("pipeline_id", pipelineEntity.Id);
                var result = pipelineEntity.ToCoreModel(pipelineItems);
                return result;
            }
            else
            {
                return null;
            }
        }

        public async Task<int> DeletePipelineAsync(string pipelineName, CancellationToken cancellationToken = default)
        {
            var pipelineRepository = _uow.GetRepository<Entities.Pipeline>();
            var result = await pipelineRepository.DeleteAsync("name", pipelineName, cancellationToken);
            return result;
        }
    }
}
