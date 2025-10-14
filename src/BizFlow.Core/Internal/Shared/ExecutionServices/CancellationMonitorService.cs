using BizFlow.Core.Contracts;
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
            var cancelService = scope.ServiceProvider.GetRequiredService<ICancelPipelineRequestService>();

            while (!linkedCts.IsCancellationRequested)
            {
                Console.WriteLine("MonitorCancellation");

                await Task.Delay(checkInterval);
                var cancellationRequest = await cancelService.GetActiveRequest(pipelineName);
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
