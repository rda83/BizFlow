
namespace BizFlow.Abstractions
{
    public interface IWorker
    {
        Task ExecuteAsync(CancellationToken cancellationToken);
    }
}
