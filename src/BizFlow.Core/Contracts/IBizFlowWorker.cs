using BizFlow.Core.Model;
using System.Text.Json;

namespace BizFlow.Core.Contracts
{
    public interface IBizFlowWorker
    {
        Task Run(WorkerContext ctx);
        Task<CheckOptionsResult> CheckOptions(JsonElement options)
        {
            return Task.FromResult(new CheckOptionsResult() { Success = true });
        }
        T? GetOptions<T>(JsonElement? options) where T : class;
    }
}
