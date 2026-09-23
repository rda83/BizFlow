
namespace BizFlow.Abstractions
{
    public interface ICancellationService
    {
        void Cancel(string pipelineName);
    }
}
