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
                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var jobsManager = scope.ServiceProvider.GetRequiredService<IJobsManager>();
                    var pipelineService = scope.ServiceProvider.GetRequiredService<IPipelineService>();

                    foreach (var item in pipelineService.GetPipelines())
                    {
                        jobsManager.CrerateJob(item.Name, item.CronExpression);
                    }
                }
            };
        }
    }
}
