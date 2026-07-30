using BizFlow.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BizFlow.ExecutionJournal.Logger
{
    public static class ServiceRegistration
    {
        public static void AddExecutionJournalLogger(this IServiceCollection services)
        {
            services.AddSingleton<IExecutionJournal, ExecutionJournalLogger>();
        }
    }
}
