using BizFlow.Core.Contracts.Storage;
using BizFlow.Core.Model.ExecutionServices;
using Microsoft.Extensions.DependencyInjection;

namespace BizFlow.Core.Internal.Shared.ExecutionServices
{
    public class CancellationMonitorService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public CancellationMonitorService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public async Task<CancellationRequestResult?> MonitorCancellationAsync(string pipelineName, CancellationTokenSource linkedCts,
            TimeSpan checkInterval)
        {
            using var scope = _scopeFactory.CreateScope();

            var storage = scope.ServiceProvider.GetRequiredService<IBizFlowStorage>();

            while (!linkedCts.IsCancellationRequested)
            {
                await Task.Delay(checkInterval);
                var cancellationRequest = await storage.GetActiveCancellationRequest(pipelineName);
                if (cancellationRequest != null)
                {
                    linkedCts.Cancel();
                    return CancellationRequestResult.FromRequest(cancellationRequest);
                }
            }
            return null;
        }
    }
}
