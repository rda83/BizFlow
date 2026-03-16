using BizFlow.Core.Model;

namespace BizFlow.Core.Contracts.Storage
{
    public interface IBizFlowStorage : IDisposable
    {
        Task AddPipelineAsync(Pipeline pipelineItem, CancellationToken cancellationToken = default);
        Task<bool> PipelineNameExistAsync(string pipelineName, CancellationToken cancellationToken = default);
        Task<(IReadOnlyCollection<Pipeline> Pipelines, long MaxId)> GetPipelinesAsync(long lastId, int limit = 100, 
            CancellationToken cancellationToken = default);
        Task<Pipeline?> GetPipelineAsync(string pipelineName, CancellationToken cancellationToken = default);
        Task<int> DeletePipelineAsync(string pipelineName, CancellationToken cancellationToken = default);
        
        Task AddJournalRecordAsync(JournalRecord record, CancellationToken cancellationToken = default);
        Task<(IReadOnlyCollection<JournalRecord> Pipelines, long MaxId)> GetJournalRecordsAsync(long lastId, int limit = 100,
            CancellationToken cancellationToken = default);
    }
}


