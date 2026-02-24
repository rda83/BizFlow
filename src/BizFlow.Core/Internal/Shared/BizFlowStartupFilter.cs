using BizFlow.Core.Contracts.Storage;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace BizFlow.Core.Internal.Shared
{
    class BizFlowStartupFilter : IStartupFilter
    {
        private const long FIRST_ID = 0;
        private const int PAGE_SIZE = 7;

        public  Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                next(app);

                using (var scope = app.ApplicationServices.CreateScope())
                {
                    var bizFlowJobManager = scope.ServiceProvider.GetRequiredService<BizFlowJobManager>();
                    var bizFlowStorage = scope.ServiceProvider.GetRequiredService<IBizFlowStorage>();

                    var lastSeenId = FIRST_ID;

                    while (true)
                    {
                        var pageResult = bizFlowStorage.GetPipelinesAsync(lastSeenId, PAGE_SIZE)
                            .GetAwaiter().GetResult();

                        foreach (var item in pageResult.Pipelines)
                        {
                            bizFlowJobManager.CrerateTrigger(item.Name, item.CronExpression)
                                .GetAwaiter().GetResult();
                        }

                        if (pageResult.Pipelines.Count < PAGE_SIZE)
                        {
                            break;
                        }

                        if (pageResult.MaxId == lastSeenId)
                        {   
                            throw new InvalidOperationException(@$"Infinite pagination loop: MaxId = {pageResult.MaxId} (no progress) 
                                while {pageResult.Pipelines.Count} items remain");
                        }

                        lastSeenId = pageResult.MaxId;
                    }
                }
            };
        }
    }
}
