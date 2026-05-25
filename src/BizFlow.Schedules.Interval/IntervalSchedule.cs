using BizFlow.Abstractions;

namespace BizFlow.Schedules.Interval
{
    public class IntervalSchedule : ISchedule
    {
        public TimeSpan Interval { get; }

        public IntervalSchedule(TimeSpan interval)
        {
            if (interval <= TimeSpan.Zero)
                throw new ArgumentException("The interval must be positive.", nameof(interval));
            Interval = interval;
        }

        public DateTimeOffset? GetNextRun(DateTimeOffset? lastRun)
        {
            return lastRun == null
                ? DateTimeOffset.UtcNow         // TODO Абстракция времени
                : lastRun.Value.Add(Interval);
        }
    }
}
