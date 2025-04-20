using BizFlow.Core.Controllers;
using BizFlow.Core.Internal;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Spi;
using Quartz.Util;
using System.Reflection;
using static Quartz.Logging.OperationName;


namespace BizFlow.Core.Services.DI
{
    public static class ServiceRegistration
    {
        public static void AddBizFlow(this IServiceCollection services,
            params Assembly[] assemblies)
        {

            #region Workers


            var interfaceType = typeof(IBizFlowWorker);
            foreach (var _assembly in assemblies)
            {
                var types = _assembly.GetTypes()
                    .Where(t => t.IsClass
                        && t.IsPublic
                        && !t.IsAbstract
                        && interfaceType.IsAssignableFrom(t));

                foreach (var _type in types)
                {
                    var columnAttribute = (TypeOperationIdAttribute?)Attribute.GetCustomAttribute(
                        _type, typeof(TypeOperationIdAttribute));


                    if (columnAttribute != null && 
                        !string.IsNullOrWhiteSpace(columnAttribute.TypeOperationId)) //TODO Необходима валидация TypeOperationId
                    {
                        services.AddKeyedScoped(typeof(IBizFlowWorker), columnAttribute.TypeOperationId, _type);
                    }
                    else
                    {
                        //TODO Информировать
                    }
                }
            }
            #endregion


            //services.AddScoped<BizFlowJob>();
            //services.AddSingleton<IJobFactory, SingletonJobFactory>();
            //services.AddSingleton<Internal.QuartzHostedService>();

            //var serviceProvider = services.BuildServiceProvider();
            //var quartzHostedService = serviceProvider.GetRequiredService<QuartzHostedService>();

            //services.AddHostedService<QuartzHostedService>(x => quartzHostedService);
            //services.AddSingleton<IJobsManager>(x => quartzHostedService);
            //services.AddHostedService<QuartzHostedService>();


            services.AddQuartz(q => {

                // your configuration here


                q.AddJob<BizFlowJob>(opts => opts
                    .WithIdentity("bizFlowDefaultJob")
                    .StoreDurably()
                );


            });



            services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);


            var assembly = typeof(BizFlowController).GetTypeInfo().Assembly;

            services.AddMvcCore()
                .AddApplicationPart(assembly);
            services.AddScoped<BizFlowJobManager>(); 
            services.AddTransient<IStartupFilter, BizFlowStartupFilter>();
        }
    }
}
