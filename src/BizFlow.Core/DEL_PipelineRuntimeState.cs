
namespace BizFlow.Core
{
    public sealed class DEL_PipelineRuntimeState
    {
        public DateTimeOffset? LastRunTime { get; set; }
        public bool IsRunning { get; set; }
        public DateTimeOffset? NextRunTime { get; set; }
        public CancellationTokenSource? RunningCTS { get; set; }
    }
}