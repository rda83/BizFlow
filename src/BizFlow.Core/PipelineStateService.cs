
using System.Collections.Concurrent;

namespace BizFlow.Core
{
    public class PipelineStateService
    {
        public bool TryAdd(string pipelineName)
        {
            bool result = false;
            lock (_lock)
            {
                var newState = new PipelineRuntimeState();
                newState.NextRunTime = null;
                newState.LastRunTime = null;

                result = _states.TryAdd(pipelineName, newState);
            }
            return result;
        }

        public bool TrySetInProgress(string pipelineName)
        {
            if (!_states.TryGetValue(pipelineName, out var state))
            {
                return false;
            }

            lock (state.LockObject)
            {
                if (!_states.TryGetValue(pipelineName, out var current) || !ReferenceEquals(current, state))
                {
                    return false;
                }

                if (state.CurrentState == PipelineState.Waiting)
                {
                    state.RunningCTS = null;
                    state.CurrentState = PipelineState.InProgress;
                    return true;
                }
                return false;
            }
        }

        public bool TrySetWaiting(string pipelineName)
        {
            if (!_states.TryGetValue(pipelineName, out var state))
            {
                return false;
            }

            lock (state.LockObject)
            {
                if (!_states.TryGetValue(pipelineName, out var current) || !ReferenceEquals(current, state))
                {
                    return false;
                }

                if (state.CurrentState == PipelineState.InProgress)
                {
                    state.CurrentState = PipelineState.Waiting;
                    state.RunningCTS = null;
                    return true;
                }
                return false;
            }
        }

        public bool TryRemove(string pipelineName)
        {
            if (!_states.TryGetValue(pipelineName, out var state))
            {
                return false;
            }

            lock (state.LockObject)
            {
                if (!_states.TryGetValue(pipelineName, out var current) || !ReferenceEquals(current, state))
                {
                    return false;
                }

                if (state.CurrentState == PipelineState.Waiting)
                {
                    if (_states.TryRemove(pipelineName, out _))
                    {
                        return true;
                    }
                }

            }
            return false;
        }

        public DateTimeOffset? GetNextRunTime(string pipelineName)
        {
            if (!_states.TryGetValue(pipelineName, out var state))
            {
                return null;
            }

            lock (state.LockObject)
            {
                if (!_states.TryGetValue(pipelineName, out var current) || !ReferenceEquals(current, state))
                {
                    return null;
                }
                return state.NextRunTime;
            }
        }

        public DateTimeOffset? GetLastRunTime(string pipelineName)
        {
            if (!_states.TryGetValue(pipelineName, out var state))
            {
                return null;
            }
            lock (state.LockObject)
            {

                if (!_states.TryGetValue(pipelineName, out var current) || !ReferenceEquals(current, state))
                {
                    return null;
                }
                return state.LastRunTime;
            }
        }

        public bool TrySetNextRunTime(string pipelineName, DateTimeOffset? nextRunTime)
        {
            if (!_states.TryGetValue(pipelineName, out var state))
            {
                return false;
            }

            lock (state.LockObject)
            {
                if (!_states.TryGetValue(pipelineName, out var current) || !ReferenceEquals(current, state))
                {
                    return false;
                }
                state.NextRunTime = nextRunTime;
            }
            return true;
        }

        public bool TrySetBeforeStartedAttributes(string pipelineName, DateTimeOffset? now, DateTimeOffset? nextRun, CancellationTokenSource cts)
        {
            if (!_states.TryGetValue(pipelineName, out var state))
            {
                return false;
            }

            lock (state.LockObject)
            {
                if (!_states.TryGetValue(pipelineName, out var current) || !ReferenceEquals(current, state))
                {
                    return false;
                }

                if (state.CurrentState != PipelineState.InProgress)
                {
                    return false;
                }

                state.LastRunTime = now;
                state.NextRunTime = nextRun;
                state.RunningCTS = cts;
                return true;
            }
        }

        public bool TryMarkCompleted(string pipelineName)
        {
            if (!_states.TryGetValue(pipelineName, out var state))
            {
                return false;
            }

            lock (state.LockObject)
            {
                if (!_states.TryGetValue(pipelineName, out var current) || !ReferenceEquals(current, state))
                {
                    return false;
                }

                if (state.CurrentState != PipelineState.InProgress)
                {
                    return false;
                }

                state.RunningCTS = null;
                state.CurrentState = PipelineState.Waiting;
                return true;
            }
        }

        public bool TryCancel(string pipelineName)
        {
            if (!_states.TryGetValue(pipelineName, out var state))
            {
                return false;
            }

            CancellationTokenSource? cts;
            lock (state.LockObject)
            {
                if (!_states.TryGetValue(pipelineName, out var current) || !ReferenceEquals(current, state))
                {
                    return false;
                }

                if (state.CurrentState != PipelineState.InProgress)
                {
                    return false;
                }

                if (state.RunningCTS == null)
                {
                    return false;
                }

                cts = state.RunningCTS;
                if (cts == null) 
                {
                    return false;
                }
            }

            try
            {
                cts?.Cancel();
                return true;
            }
            catch (ObjectDisposedException)
            {
                return false;
            }
        }

        public DateTimeOffset?[] GetAllNextRunTimes()
        {
            var snapshot = _states.ToArray();
            var result = new DateTimeOffset?[snapshot.Length];

            for (int i = 0; i < snapshot.Length; i++)
            {
                lock (snapshot[i].Value.LockObject)
                {
                    result[i] = snapshot[i].Value.NextRunTime;
                }
            }
            return result;
        }

        private readonly object _lock = new object();
        private readonly ConcurrentDictionary<string, PipelineRuntimeState> _states = new();
        private sealed class PipelineRuntimeState
        {
            public object LockObject { get; } = new object();
            public DateTimeOffset? LastRunTime { get; set; }
            public PipelineState CurrentState { get; set; } = PipelineState.Waiting;

            public DateTimeOffset? NextRunTime { get; set; }
            public CancellationTokenSource? RunningCTS { get; set; }
        }
        private enum PipelineState
        {
            Waiting,
            InProgress
        }
    }
}
