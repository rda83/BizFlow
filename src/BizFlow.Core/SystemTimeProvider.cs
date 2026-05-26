using BizFlow.Abstractions;

namespace BizFlow.Core
{
    public sealed class SystemTimeProvider : ITimeProvider
    {
        public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
    }
}
