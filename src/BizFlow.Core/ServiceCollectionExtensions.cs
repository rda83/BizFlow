using Microsoft.Extensions.DependencyInjection;

namespace BizFlow.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBizFlowCore(this IServiceCollection services)
        {
            services.AddSingleton<PipelineStateService>();
            services.AddSingleton<BizFlowScheduler>();
            services.AddHostedService<BizFlowScheduler>(sp => sp.GetRequiredService<BizFlowScheduler>());
            return services;
        }
    }
}
