
namespace BizFlow.Core
{
    public interface IPipelineService
    {
        IReadOnlyCollection<Pipeline> GetPipelines();
        //Task<IReadOnlyCollection<Pipeline>> GetPipelinesAsync(CancellationToken cancellationToken = default);
    }
}
