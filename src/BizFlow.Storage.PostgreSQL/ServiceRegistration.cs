using BizFlow.Core.Contracts.Storage;
using BizFlow.Storage.PostgreSQL.Entities;
using BizFlow.Storage.PostgreSQL.Infrastructure;
using BizFlow.Storage.PostgreSQL.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace BizFlow.Storage.PostgreSQL
{
    public static class ServiceRegistration
    {
        public static void AddPostgreSQLBizFlowStorage(this IServiceCollection services)
        {

            services.AddSingleton(sp => new ConnectionFactory("Host=localhost;Port=5432;Database=mydb;Username=client-biz-flow;Password=mysecretpassword"));

            services.AddScoped<IRepository<Pipeline>, PipelineRepository>();
            services.AddScoped<IRepository<PipelineItem>, PipelineItemRepository>();

            services.AddScoped<UnitOfWork>();
            services.AddScoped<IBizFlowStorage, PostgreSQLBizFlowStorage>();
        }
    }
}
