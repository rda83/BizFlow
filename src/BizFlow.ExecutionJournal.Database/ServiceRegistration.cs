using BizFlow.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace BizFlow.ExecutionJournal.Database
{
    public static class ServiceRegistration
    {
        public static void AddExecutionJournalDatabase(this IServiceCollection services)
        {
            services.AddSingleton<IExecutionJournal, ExecutionJournalDatabase>();
        }
    }
}
