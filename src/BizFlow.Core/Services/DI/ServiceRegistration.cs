using BizFlow.Core.Controllers;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BizFlow.Core.Services.DI
{
    public static class ServiceRegistration
    {
        public static void AddBizFlow(this IServiceCollection services)
        {
            var assembly = typeof(BizFlowController).GetTypeInfo().Assembly;

            services.AddMvcCore()
                .AddApplicationPart(assembly);
        }
    }
}
