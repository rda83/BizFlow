using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


// - нужен отдельный сервис синглтон - хранилище пайплайнов
namespace BizFlow.Abstractions
{
    internal class IBizFlowPipelineService
    {
        // public async Task CreatePipeline(string name, string cronExpression, CancellationToken ct = default)
        // public async Task CreatePipeline(string name, TimeSpan interval, CancellationToken ct = default)



        // public async Task DeleteTrigger(string pipelineName, CancellationToken cancellationToken = default)
        // public async Task StartNow(string pipelineName, string launchId, CancellationToken cancellationToken = default)
        // public async Task<bool> TriggerCheckExists(string triggerName, CancellationToken cancellationToken = default)
    }
}
