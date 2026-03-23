using BizFlow.Core.Model;

namespace BizFlow.Core.Contracts
{
    public interface IBizFlowJournal
    {
        Task<string?> GetLastLaunchId(string pipelineName, CancellationToken cancellationToken = default);
    }
}
