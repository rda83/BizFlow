
using System.Collections.Concurrent;

namespace BizFlow.Core
{
    /// <summary>
    /// Manages the thread-safe runtime state of registered pipelines.
    /// Tracks execution status, last and next run times, and active cancellation sources.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class is thread-safe. All public methods may be called concurrently
    /// from multiple threads.
    /// </para>
    /// <para>
    /// To prevent race conditions when a pipeline is removed and re-added under
    /// the same name, all state-mutating methods verify that the internal state
    /// object is still the current one for the given name before applying changes.
    /// </para>
    /// </remarks>
    public class PipelineStateService
    {
        /// <summary>
        /// Registers a new runtime state for the specified pipeline name.
        /// </summary>
        /// <param name="pipelineName">The unique name of the pipeline.</param>
        /// <returns>
        /// <see langword="true"/> if the state was successfully added;
        /// <see langword="false"/> if a state with the same name already exists.
        /// </returns>
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

        /// <summary>
        /// Attempts to transition the pipeline state from <c>Waiting</c> to <c>InProgress</c>.
        /// </summary>
        /// <param name="pipelineName">The unique name of the pipeline.</param>
        /// <returns>
        /// <see langword="true"/> if the transition was successful;
        /// <see langword="false"/> if the pipeline does not exist, has been removed,
        /// or is already in the <c>InProgress</c> state.
        /// </returns>
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

        /// <summary>
        /// Attempts to transition the pipeline state from <c>InProgress</c> to <c>Waiting</c>.
        /// </summary>
        /// <param name="pipelineName">The unique name of the pipeline.</param>
        /// <returns>
        /// <see langword="true"/> if the transition was successful;
        /// <see langword="false"/> if the pipeline does not exist, has been removed,
        /// or is not currently in the <c>InProgress</c> state.
        /// </returns>
        /// <remarks>
        /// Upon successful transition, the active <see cref="CancellationTokenSource"/>
        /// (if any) is cleared.
        /// </remarks>
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

        /// <summary>
        /// Removes the runtime state for the specified pipeline.
        /// </summary>
        /// <param name="pipelineName">The unique name of the pipeline.</param>
        /// <returns>
        /// <see langword="true"/> if the state was successfully removed;
        /// <see langword="false"/> if the pipeline does not exist or is currently
        /// in the <c>InProgress</c> state.
        /// </returns>
        /// <remarks>
        /// Only pipelines in the <c>Waiting</c> state can be removed. An executing
        /// pipeline must be completed or cancelled before its state can be removed.
        /// </remarks>
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

        /// <summary>
        /// Gets the scheduled next run time for the specified pipeline.
        /// </summary>
        /// <param name="pipelineName">The unique name of the pipeline.</param>
        /// <returns>
        /// The <see cref="DateTimeOffset"/> of the next scheduled run, or
        /// <see langword="null"/> if the pipeline does not exist or the next run time
        /// has not been calculated yet.
        /// </returns>
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

        /// <summary>
        /// Gets the last run time for the specified pipeline.
        /// </summary>
        /// <param name="pipelineName">The unique name of the pipeline.</param>
        /// <returns>
        /// The <see cref="DateTimeOffset"/> of the last run, or <see langword="null"/>
        /// if the pipeline does not exist or has never been run.
        /// </returns>
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

        /// <summary>
        /// Sets the next run time for the specified pipeline.
        /// </summary>
        /// <param name="pipelineName">The unique name of the pipeline.</param>
        /// <param name="nextRunTime">The calculated next run time, or <see langword="null"/> to indicate no next run time.</param>
        /// <returns>
        /// <see langword="true"/> if the value was updated;
        /// <see langword="false"/> if the pipeline does not exist or has been removed.
        /// </returns>
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

        /// <summary>
        /// Updates the runtime attributes of a pipeline right before its execution starts.
        /// </summary>
        /// <param name="pipelineName">The unique name of the pipeline.</param>
        /// <param name="now">The current time, used as the last run time.</param>
        /// <param name="nextRun">The calculated next run time.</param>
        /// <param name="cts">The cancellation token source associated with this execution.</param>
        /// <returns>
        /// <see langword="true"/> if the attributes were updated;
        /// <see langword="false"/> if the pipeline does not exist, has been removed,
        /// or is not in the <c>InProgress</c> state.
        /// </returns>
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

        /// <summary>
        /// Marks the pipeline execution as completed and transitions the state back to <c>Waiting</c>.
        /// </summary>
        /// <param name="pipelineName">The unique name of the pipeline.</param>
        /// <returns>
        /// <see langword="true"/> if the transition was successful;
        /// <see langword="false"/> if the pipeline does not exist, has been removed,
        /// or is not in the <c>InProgress</c> state.
        /// </returns>
        /// <remarks>
        /// Clears the active <see cref="CancellationTokenSource"/> if the transition succeeds.
        /// </remarks>
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

        /// <summary>
        /// Attempts to cancel the currently running execution of the specified pipeline.
        /// </summary>
        /// <param name="pipelineName">The unique name of the pipeline.</param>
        /// <returns>
        /// <see langword="true"/> if the cancellation request was successfully issued;
        /// <see langword="false"/> if the pipeline does not exist, is not in the <c>InProgress</c> state,
        /// has no active cancellation source, or if the source was already disposed.
        /// </returns>
        /// <remarks>
        /// The <see cref="CancellationTokenSource.Cancel()"/> method is invoked outside the internal
        /// state lock to prevent deadlocks if cancellation callbacks try to access the state service.
        /// </remarks>
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
                // The task completed and disposed its CTS concurrently with our cancellation attempt.
                // The task is already finished, so there is nothing left to cancel.
                return false;
            }
        }

        /// <summary>
        /// Gets a snapshot of all <c>Waiting</c> pipelines that have a non-null next run time.
        /// </summary>
        /// <returns>
        /// An array of calculated next run times, suitable for determining the scheduler's next wake-up delay.
        /// </returns>
        /// <remarks>
        /// This method is lock-free with respect to individual pipeline states. It takes a point-in-time
        /// snapshot of the dictionary and filters the results. Some states might change immediately after
        /// the snapshot is taken, but this is acceptable for delay calculation purposes.
        /// </remarks>
        public DateTimeOffset[] GetWaitingItemsNextNotNullRunTimes()
        {
            var snapshot = _states.ToArray();

            var result = snapshot
                .Where(i => i.Value.CurrentState == PipelineState.Waiting)
                .Where(i => i.Value.NextRunTime != null)
                .Select(i => i.Value.NextRunTime!.Value)
                .ToArray();

            return result;
        }

        private readonly object _lock = new object();
        private readonly ConcurrentDictionary<string, PipelineRuntimeState> _states = new();

        /// <summary>
        /// Represents the mutable, thread-safe (via <see cref="LockObject"/>) runtime state of a single pipeline.
        /// </summary>
        private sealed class PipelineRuntimeState
        {
            public object LockObject { get; } = new object();
            public DateTimeOffset? LastRunTime { get; set; }
            public PipelineState CurrentState { get; set; } = PipelineState.Waiting;

            public DateTimeOffset? NextRunTime { get; set; }
            public CancellationTokenSource? RunningCTS { get; set; }
        }

        /// <summary>
        /// Represents the execution status of a pipeline.
        /// </summary>
        private enum PipelineState
        {
            /// <summary>The pipeline is idle and waiting for its next scheduled run time.</summary>
            Waiting,

            /// <summary>The pipeline is currently executing.</summary>
            InProgress
        }
    }
}
