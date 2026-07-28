using BizFlow.Abstractions;
using BizFlow.Abstractions.Model;
using BizFlow.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BizFlow.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static void AddBizFlowWorkers(this IServiceCollection services,
            params Assembly[] assemblies)
        {
            var interfaceType = typeof(IWorker);
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
                        services.AddKeyedScoped(typeof(IWorker), columnAttribute.TypeOperationId, _type);
                    }
                    else
                    {
                        //TODO Информировать
                    }
                }
            }
        }

        


        public static IServiceCollection AddBizFlowScheduler(this IServiceCollection services,
            Action<BizFlowSchedulerOptions>? configure = null)
        {
            var options = new BizFlowSchedulerOptions();
            configure?.Invoke(options);


            //services.AddSingleton<IBizFlowPipelineService>

            services.AddSingleton<ITimeProvider>(options.TimeProvider ?? new SystemTimeProvider());

            services.AddHostedService<BizFlowScheduler>(); // Использует коллекцию JobDefinition через DI
            return services;
        }



        // AddWorker -> AddPipeline (получается коллекция расписаний в памяти), возможно это сервис какой то должен быть
        // его будет запрашивать BizFlowScheduler в своем цикле
        public static IServiceCollection AddWorker(this IServiceCollection services, string name,
            Func<IServiceProvider, IWorker> workerFactory, Func<IServiceProvider, ISchedule> scheduleFactory)
        {
            services.AddSingleton(sp => new JobDefinition(name,
                workerFactory(sp), scheduleFactory(sp)));

            return services;
        }


    }
}
