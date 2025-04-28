using BizFlow.Core.Contracts;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BizFlow.Core.Internal.Shared
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
                    var bizFlowJobManager = scope.ServiceProvider.GetRequiredService<BizFlowJobManager>();
                    var pipelineService = scope.ServiceProvider.GetRequiredService<IPipelineService>();

                    foreach (var item in pipelineService.GetPipelines())
                    {
                        bizFlowJobManager.CrerateTrigger(item.Name, item.CronExpression)
                            .GetAwaiter()
                            .GetResult();
                    }
                }
            };
        }
    }
}
