using BizFlow.Abstractions;

namespace BizFlow.Extensions.DependencyInjection
{
    public class BizFlowSchedulerOptions
    {
        public ITimeProvider? TimeProvider { get; set; }
        public TimeSpan PollingInterval { get; set; } = TimeSpan.FromSeconds(1);
    }
}
