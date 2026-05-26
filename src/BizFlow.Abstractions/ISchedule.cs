
namespace BizFlow.Abstractions
{
    public interface ISchedule
    {
        DateTimeOffset? GetNextRun(DateTimeOffset? lastRun, DateTimeOffset now);
    }
}
