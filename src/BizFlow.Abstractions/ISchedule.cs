
namespace BizFlow.Abstractions
{
    /// <summary>
    /// Defines a contract for calculating the next execution time of a pipeline.
    /// </summary>
    /// <remarks>
    /// Implementations of this interface define the logic for various scheduling strategies 
    /// (e.g., fixed intervals, CRON expressions).
    /// </remarks>
    public interface ISchedule
    {
        /// <summary>
        /// Calculates the next scheduled run time based on the last run time and the current time.
        /// </summary>
        /// <param name="lastRun">
        /// The time of the previous execution, or <see langword="null"/> if the pipeline has never been executed.
        /// </param>
        /// <param name="now">The current time.</param>
        /// <returns>
        /// The <see cref="DateTimeOffset"/> of the next scheduled run, or <see langword="null"/> 
        /// if the schedule cannot determine a next run time (e.g., a CRON expression that has no future occurrences).
        /// </returns>
        DateTimeOffset? GetNextRun(DateTimeOffset? lastRun, DateTimeOffset now);
    }
}
