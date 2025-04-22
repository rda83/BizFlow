using BizFlow.Core.Model;

namespace BizFlow.Core.Contracts
{
    public interface IBizFlowWorker
    {
        Task Run(WorkerContext ctx);
    }
}
