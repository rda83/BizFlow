using BizFlow.Core.Contracts.Storage;
using BizFlow.Storage.PostgreSQL.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace BizFlow.Storage.PostgreSQL
{
    public static class ServiceRegistration
    {
        public static void AddPostgreSQLBizFlowStorage(this IServiceCollection services)
        {

            services.AddSingleton(sp => new ConnectionFactory("Host=localhost;Port=5432;Database=mydb;Username=myuser;Password=mysecretpassword"));

            services.AddScoped<IBizFlowStorage, PostgreSQLBizFlowStorage>();
        }
    }
}
