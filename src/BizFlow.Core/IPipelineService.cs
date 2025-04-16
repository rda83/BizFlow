
namespace BizFlow.Core
{
    public interface IPipelineService
    {
        IReadOnlyCollection<Pipeline> GetPipelines();
        Pipeline GetPipeline(string pipelineName);
    }
}
