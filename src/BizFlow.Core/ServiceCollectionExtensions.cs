using BizFlow.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BizFlow.Core
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddBizFlowCore(this IServiceCollection services)
        {
            // Внутренния сервисы ядра
            services.AddSingleton<PipelineStateService>();
            services.TryAddSingleton<IPipelineRegistry, DefaultPipelineRegistry>();
            services.AddScoped<PipelineExecutor>();

            // Журнал по умолчанию
            services.TryAddSingleton<IExecutionJournal, NullExecutionJournal>();

            // Тайм провайдер по умолчанию
            services.TryAddSingleton<ITimeProvider, SystemTimeProvider>();

            // Регистрация планировщика как HostedService
            // регистрируем как Singleton, чтобы иметь доступ к ICancellationService

            services.AddSingleton<BizFlowScheduler>();
            services.AddHostedService(sp => sp.GetRequiredService<BizFlowScheduler>());

            // Регистрируем интерфейс отмены выполнения, который смотрит на тот же экземпляр
            services.AddSingleton<ICancellationService>(sp => sp.GetRequiredService<BizFlowScheduler>());

            return services;
        }
    }
}
