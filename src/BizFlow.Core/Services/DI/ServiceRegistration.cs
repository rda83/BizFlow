using BizFlow.Core.Contracts;
using BizFlow.Core.Controllers;
using BizFlow.Core.Internal.Features.AddPipeline;
using BizFlow.Core.Internal.Jobs;
using BizFlow.Core.Internal.Shared;
using BizFlow.Core.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System.Reflection;


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
            services.AddScoped<PipelineExecutor>();
            services.AddScoped<IAddPipelineHandler, AddPipelineHandler>();
            services.AddTransient<IStartupFilter, BizFlowStartupFilter>();
        }
    }
}
