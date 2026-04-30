using BizFlow.Core.Contracts.Storage;
using BizFlow.Storage.PostgreSQL.Entities;
using BizFlow.Storage.PostgreSQL.Infrastructure;
using BizFlow.Storage.PostgreSQL.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace BizFlow.Storage.PostgreSQL
{
    public static class ServiceRegistration
    {
        public static void AddPostgreSQLBizFlowStorage(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton(sp => new ConnectionFactory(connectionString));

            services.AddScoped<IRepository<Pipeline>, PipelineRepository>();
            services.AddScoped<IRepository<PipelineItem>, PipelineItemRepository>();
            services.AddScoped<IRepository<JournalRecord>, JournalRecordRepository>();
            services.AddScoped<IRepository<CancellationRequest>, CancellationRequestRepository>();

            services.AddScoped<UnitOfWork>();
            services.AddScoped<IBizFlowStorage, PostgreSQLBizFlowStorage>();
        }
    }
}
