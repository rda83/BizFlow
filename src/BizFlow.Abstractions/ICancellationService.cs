
namespace BizFlow.Abstractions
{
    public interface ICancellationService
    {
        bool Cancel(string pipelineName);
    }
}
