using BizFlow.Core.Model;

namespace BizFlow.Core.Contracts
{
    public interface IBizFlowJournal
    {
        Task AddRecordAsync(JournalRecord record, CancellationToken cancellationToken = default);
        Task<IEnumerable<JournalRecord>> GetPagedAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);
        Task<IEnumerable<JournalRecord>> GetJournalRecordByLaunchId(string launchId, CancellationToken cancellationToken = default);
        Task<string?> GetLastLaunchId(string pipelineName, CancellationToken cancellationToken = default);
    }
}
