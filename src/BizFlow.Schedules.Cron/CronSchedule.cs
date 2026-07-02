using BizFlow.Abstractions;
using Cronos;

namespace BizFlow.Schedules.Cron
{
    public class CronSchedule : ISchedule
    {
        private readonly CronExpression _expression;
        private readonly TimeZoneInfo _timeZone;

        public CronSchedule(string cronExpression, TimeZoneInfo? timeZone = null)
        {
            _expression = CronExpression.Parse(cronExpression);
            _timeZone = timeZone ?? TimeZoneInfo.Utc;
        }

        public DateTimeOffset? GetNextRun(DateTimeOffset? lastRun, DateTimeOffset now)
        {
            if (lastRun == null)
            {
                return _expression.GetNextOccurrence(now, _timeZone);
            }
            return _expression.GetNextOccurrence(lastRun.Value, _timeZone);
        }
    }
}
