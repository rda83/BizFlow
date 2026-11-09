using BizFlow.Abstractions;

namespace BizFlow.Schedules.Interval
{
    /// <summary>
    /// A schedule implementation that executes pipelines at a fixed time interval.
    /// </summary>
    public class IntervalSchedule : ISchedule
    {
        /// <summary>
        /// Gets the fixed time interval between pipeline executions.
        /// </summary>
        public TimeSpan Interval { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntervalSchedule"/> class.
        /// </summary>
        /// <param name="interval">The time interval between executions. Must be greater than <see cref="TimeSpan.Zero"/>.</param>
        /// <exception cref="ArgumentException">
        /// Thrown if <paramref name="interval"/> is less than or equal to <see cref="TimeSpan.Zero"/>.
        /// </exception>
        public IntervalSchedule(TimeSpan interval)
        {
            if (interval <= TimeSpan.Zero)
            {
                throw new ArgumentException("The interval must be positive.", nameof(interval));
            }

            Interval = interval;
        }

        /// <summary>
        /// Calculates the next scheduled run time based on the fixed interval.
        /// </summary>
        /// <param name="lastRun">The time of the previous execution.</param>
        /// <param name="now">The current time.</param>
        /// <returns>
        /// If <paramref name="lastRun"/> is <see langword="null"/>, returns <paramref name="now"/> 
        /// (indicating an immediate first run). Otherwise, returns <paramref name="lastRun"/> plus the <see cref="Interval"/>.
        /// </returns>
        public DateTimeOffset? GetNextRun(DateTimeOffset? lastRun, DateTimeOffset now)
        {
            return lastRun == null ? now : lastRun.Value.Add(Interval);
        }
    }
}
