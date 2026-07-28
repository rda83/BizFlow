using BizFlow.Abstractions.Model;
using System.Text.Json;

namespace BizFlow.Core.Contracts
{
    public interface DEL_IBizFlowWorker
    {
        Task Run(WorkerContext ctx);
        Task<CheckOptionsResult> CheckOptions(JsonElement options)
        {
            return Task.FromResult(new CheckOptionsResult() { Success = true });
        }
        T? GetOptions<T>(JsonElement? options) where T : class;
    }
}
