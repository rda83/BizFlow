
namespace BizFlow.Abstractions
{
    /// <summary>
    /// Provides an abstraction for retrieving the current time.
    /// </summary>
    /// <remarks>
    /// This interface is primarily used to decouple the scheduler from the system clock,
    /// making it possible to use deterministic time providers (e.g., fake clocks) in unit tests.
    /// </remarks>
    public interface ITimeProvider
    {
        /// <summary>
        /// Gets the current Coordinated Universal Time (UTC).
        /// </summary>
        /// <value>
        /// A <see cref="DateTimeOffset"/> representing the current UTC time.
        /// </value>
        DateTimeOffset UtcNow { get; }
    }
}
