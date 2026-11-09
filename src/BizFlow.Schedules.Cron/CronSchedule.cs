using BizFlow.Abstractions;
using Cronos;

namespace BizFlow.Schedules.Cron
{
    /// <summary>
    /// A schedule implementation that calculates execution times based on a CRON expression.
    /// </summary>
    public class CronSchedule : ISchedule
    {
        private readonly CronExpression _expression;
        private readonly TimeZoneInfo _timeZone;

        /// <summary>
        /// Initializes a new instance of the <see cref="CronSchedule"/> class.
        /// </summary>
        /// <param name="cronExpression">A standard CRON expression defining the schedule (e.g., "0 * * * *" for hourly).</param>
        /// <param name="timeZone">
        /// The time zone in which the CRON expression should be evaluated. 
        /// If <see langword="null"/>, defaults to Coordinated Universal Time (UTC).
        /// </param>
        /// <exception cref="CronFormatException">
        /// Thrown if the provided <paramref name="cronExpression"/> is not a valid CRON format.
        /// </exception>
        public CronSchedule(string cronExpression, TimeZoneInfo? timeZone = null)
        {
            _expression = CronExpression.Parse(cronExpression);
            _timeZone = timeZone ?? TimeZoneInfo.Utc;
        }

        /// <summary>
        /// Calculates the next scheduled run time based on the CRON expression.
        /// </summary>
        /// <param name="lastRun">The time of the previous execution, or <see langword="null"/> if never run.</param>
        /// <param name="now">The current time.</param>
        /// <returns>
        /// The next occurrence according to the CRON expression and the specified time zone, 
        /// or <see langword="null"/> if no future occurrences exist.
        /// </returns>
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
