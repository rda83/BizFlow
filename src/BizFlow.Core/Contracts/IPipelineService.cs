using BizFlow.Core.Model;

namespace BizFlow.Core.Contracts
{
    public interface IPipelineService
    {
        IReadOnlyCollection<Pipeline> GetPipelines();
        Pipeline GetPipeline(string pipelineName);
    }
}
