
using System.Collections.Concurrent;

namespace BizFlow.Core
{
    public class PipelineStateService
    {
        private readonly object _lock = new object();
        private readonly ConcurrentDictionary<string, PipelineRuntimeState> _states = new();

        public bool TryAdd(string pipelineName)
        {
            bool result = false;
            lock (_lock)
            {
                var newState = new PipelineRuntimeState();
                newState.NextRunTime = new DateTimeOffset(DateTime.MinValue.ToUniversalTime());
                newState.LastRunTime = new DateTimeOffset(DateTime.MinValue.ToUniversalTime());

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
                if (_states.ContainsKey(pipelineName))
                {
                    if (state.CurrentState == PipelineState.Waiting)
                    {
                        state.CurrentState = PipelineState.InProgress;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
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
                if (_states.ContainsKey(pipelineName))
                {
                    if (state.CurrentState == PipelineState.InProgress)
                    {
                        state.CurrentState = PipelineState.Waiting;
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
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
                if (_states.ContainsKey(pipelineName))
                {
                    if (state.CurrentState == PipelineState.Waiting)
                    {
                        if(_states.TryRemove(pipelineName, out _))
                        {
                            return true;
                        }
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
                if (!_states.ContainsKey(pipelineName))
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
                if (!_states.ContainsKey(pipelineName))
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
                if (!_states.ContainsKey(pipelineName))
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
                if (!_states.ContainsKey(pipelineName))
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
                if (!_states.ContainsKey(pipelineName))
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

            lock (state.LockObject)
            {
                if (!_states.ContainsKey(pipelineName))
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

                state.RunningCTS.Cancel();
                return true;
            }       
        }

        public DateTimeOffset?[] GetAllNextRunTimes()
        {
            return _states.Select(i => i.Value.NextRunTime).ToArray();
        }


        private sealed class PipelineRuntimeState
        {
            public object LockObject { get; } = new object();
            public DateTimeOffset? LastRunTime { get; set; }
            
            //public bool IsRunning { get; set; }

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
