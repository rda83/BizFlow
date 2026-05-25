using BizFlow.Abstractions;
using BizFlow.Core;
using Microsoft.Extensions.DependencyInjection;

namespace BizFlow.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBizFlowScheduler(this IServiceCollection services)
        {
            services.AddHostedService<BizFlowScheduler>();
            return services;
        }

        public static IServiceCollection AddWorker(this IServiceCollection services, string name,
            Func<IServiceProvider, IWorker> workerFactory, Func<IServiceProvider, ISchedule> scheduleFactory)
        {
            services.AddSingleton(sp => new JobDefinition(name,
                workerFactory(sp), scheduleFactory(sp)));

            return services;
        }


    }
}
