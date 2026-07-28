
using BizFlow.Abstractions.Model;
using System.Text.Json;

namespace BizFlow.Abstractions
{
    public interface IWorker 
    {
        Task ExecuteAsync(WorkerContext ctx, CancellationToken ct);
        Task<CheckOptionsResult> CheckOptions(JsonElement options)
        {
            return Task.FromResult(new CheckOptionsResult() { Success = true });
        }
        T? GetOptions<T>(JsonElement? options) where T : class;
    }
}
