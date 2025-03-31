using BizFlow.Core.Controllers;
using BizFlow.Core.Internal;
using Microsoft.Extensions.DependencyInjection;
using Quartz.Spi;
using System.Reflection;


namespace BizFlow.Core.Services.DI
{
    public static class ServiceRegistration
    {
        public static void AddBizFlow(this IServiceCollection services)
        {
            services.AddSingleton<BizFlowJob>();
            services.AddSingleton<IJobFactory, SingletonJobFactory>();
            services.AddSingleton<Internal.QuartzHostedService>();

            var serviceProvider = services.BuildServiceProvider();
            var quartzHostedService = serviceProvider.GetRequiredService<QuartzHostedService>();

            services.AddHostedService<QuartzHostedService>(x => quartzHostedService);
            services.AddSingleton<IJobsManager>(x => quartzHostedService);
            services.AddHostedService<QuartzHostedService>();

            var assembly = typeof(BizFlowController).GetTypeInfo().Assembly;

            services.AddMvcCore()
                .AddApplicationPart(assembly);

        }
    }
}
