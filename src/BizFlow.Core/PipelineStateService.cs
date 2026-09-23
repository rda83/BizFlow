
using System.Collections.Concurrent;

namespace BizFlow.Core
{
    public class PipelineStateService
    {
        private readonly object _lock = new object();
        private readonly ConcurrentDictionary<string, PipelineRuntimeState> _states = new();

        public void Add(string pipelineName)
        {
            lock (_lock)
            {
                _states.TryAdd(pipelineName, new PipelineRuntimeState());
            }
        }

        public bool IsRunning(string pipelineName)
        {
            return _states[pipelineName].IsRunning;
        }

        public void MarkStarted(string pipelineName, DateTimeOffset? now, DateTimeOffset? nextRun, CancellationTokenSource cts)
        {
            var pipelineState = _states[pipelineName];

            lock (pipelineState.LockObject)
            {
                pipelineState.IsRunning = true;
                pipelineState.LastRunTime = now;
                pipelineState.NextRunTime = nextRun;
                pipelineState.RunningCTS = cts;
            }
        }

        public void MarkCompleted(string pipelineName)
        {
            var pipelineState = _states[pipelineName];
            lock (pipelineState.LockObject)
            {
                pipelineState.IsRunning = false;
                pipelineState.RunningCTS = null;
            }
        }

        public void Cancel(string pipelineName)
        {
            var pipelineState = _states[pipelineName];
            lock (pipelineState.LockObject)
            {
                pipelineState.RunningCTS?.Cancel();
            }
        }

        public void Remove(string pipelineName)
        {
            var pipelineState = _states[pipelineName];
            lock (pipelineState.LockObject)
            {
                _states.TryRemove(pipelineName, out _);
            }
        }

        public DateTimeOffset?[] GetAllNextRunTimes()
        {
            return _states.Select(i => i.Value.NextRunTime).ToArray();
        }

        public DateTimeOffset? GetNextRunTime(string pipelineName)
        {
            return _states[pipelineName].NextRunTime;
        }

        public void SetNextRunTime(string pipelineName, DateTimeOffset? nextRunTime)
        {
            _states[pipelineName].NextRunTime = nextRunTime;
        }

        public DateTimeOffset? GetLastRunTime(string pipelineName)
        {
            return _states[pipelineName].LastRunTime;
        }

        private sealed class PipelineRuntimeState
        {
            public object LockObject { get; } = new object();
            public DateTimeOffset? LastRunTime { get; set; }
            public bool IsRunning { get; set; }
            public DateTimeOffset? NextRunTime { get; set; }
            public CancellationTokenSource? RunningCTS { get; set; }
        }
    }
}
