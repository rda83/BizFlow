using BizFlow.Core.Controllers;
using BizFlow.Core.Internal;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System.Reflection;

namespace BizFlow.Core.Services.DI
{
    public static class ServiceRegistration
    {
        public static void AddBizFlow(this IServiceCollection services)
        {
            services.AddQuartz(q =>
            {
                var jobKey = new JobKey("BizFlowJob");
                q.AddJob<BizFlowJob>(opts => opts.WithIdentity(jobKey));

                q.AddTrigger(opts => opts
                    .ForJob(jobKey)
                    .WithIdentity("BizFlowJob-trigger")
                    .WithCronSchedule("0/5 * * * * ?"));
            });

            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            var assembly = typeof(BizFlowController).GetTypeInfo().Assembly;

            services.AddMvcCore()
                .AddApplicationPart(assembly);
        }
    }
}
