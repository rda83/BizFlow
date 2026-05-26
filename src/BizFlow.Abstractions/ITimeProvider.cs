
namespace BizFlow.Abstractions
{
    public interface ITimeProvider
    {
        DateTimeOffset UtcNow { get; }
    }
}
