using BizFlow.Abstractions;

namespace BizFlow.Core
{
    /// <summary>
    /// Default implementation of <see cref="ITimeProvider"/> that retrieves the current time
    /// directly from the system clock.
    /// </summary>
    public sealed class SystemTimeProvider : ITimeProvider
    {
        /// <inheritdoc/>
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
