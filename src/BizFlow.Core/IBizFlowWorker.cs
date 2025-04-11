
namespace BizFlow.Core
{
    public interface IBizFlowWorker
    {
        Task Run(WorkerContext ctx);
    }
}
