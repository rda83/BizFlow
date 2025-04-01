using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BizFlow.Core.Internal
{
    class BizFlowStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                next(app);
                
                var jobsManager = app.ApplicationServices.GetRequiredService<IJobsManager>();
                jobsManager.CrerateJob("BizFlowJob",
                    "0/10 * * * * ?");
            };
        }
    }
}
