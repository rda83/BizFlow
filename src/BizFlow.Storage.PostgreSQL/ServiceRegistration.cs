using BizFlow.Core.Contracts.Storage;
using BizFlow.Storage.PostgreSQL.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BizFlow.Storage.PostgreSQL
{
    public static class ServiceRegistration
    {
        public static void AddPostgreSQLBizFlowStorage(this IServiceCollection services)
        {

            services.AddSingleton(sp => new ConnectionFactory(""));

            services.AddScoped<IBizFlowStorage, PostgreSQLBizFlowStorage>();
        }
    }
}
