using BizFlow.Core.Model;

namespace BizFlow.Core.Contracts
{
    public interface IBizFlowJournal
    {
        Task<IEnumerable<JournalRecord>> GetJournalRecordByLaunchId(string launchId, CancellationToken cancellationToken = default);
        Task<string?> GetLastLaunchId(string pipelineName, CancellationToken cancellationToken = default);
    }
}
