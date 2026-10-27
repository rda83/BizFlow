using BizFlow.Abstractions;
using BizFlow.Abstractions.Model;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace BizFlow.Core
{
    /// <summary>
    /// Default implementation of <see cref="IPipelineRegistry"/> backed by an
    /// in-memory <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey,TValue}"/>
    /// for pipeline definitions and a <see cref="PipelineStateService"/> for
    /// runtime state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Thread-safe. All interface members may be called concurrently from
    /// multiple threads.
    /// </para>
    /// <para>
    /// <see cref="Create"/> and <see cref="Remove"/> mutate both the pipeline
    /// definitions and the <see cref="PipelineStateService"/> under a shared
    /// lock, so the two stores remain consistent with respect to each other
    /// for these operations. If the second operation fails after the first
    /// succeeded, the change is rolled back (where possible) and the
    /// inconsistency is reported through logging.
    /// </para>
    /// <para>
    /// Read operations (<see cref="GetAll"/>, <see cref="Exist"/>) do not take
    /// the lock. <see cref="GetAll"/> returns a snapshot of the definition
    /// dictionary; <see cref="Exist"/> reads the dictionary directly.
    /// </para>
    /// <para>
    /// <see cref="IPipelineRegistry.OnAdded"/> and <see cref="IPipelineRegistry.OnRemoved"/>
    /// are invoked outside the lock so that a slow or re-entrant handler cannot
    /// stall writers or cause a deadlock. As a consequence, the ordering of
    /// these events is not guaranteed under concurrent <see cref="Create"/> /
    /// <see cref="Remove"/> calls; see <see cref="IPipelineRegistry.OnAdded"/>
    /// for details.
    /// </para>
    /// </remarks>
    public class DefaultPipelineRegistry : IPipelineRegistry
    {
        private readonly ILogger<DefaultPipelineRegistry> _logger;
        private readonly PipelineStateService _pipelineStateService;
        private readonly ConcurrentDictionary<string, Pipeline> _pipelineDefinitions = new();
        private readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultPipelineRegistry"/> class.
        /// </summary>
        /// <param name="pipelineStateService">
        /// The state service used to store runtime state for registered pipelines.
        /// Must not be <see langword="null"/>.
        /// </param>
        /// <param name="logger">
        /// The logger used to report invariant violations and subscriber exceptions.
        /// Must not be <see langword="null"/>.
        /// </param>
        public DefaultPipelineRegistry(
            PipelineStateService pipelineStateService,
            ILogger<DefaultPipelineRegistry> logger)
        {
            _pipelineStateService = pipelineStateService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public event EventHandler<Pipeline>? OnAdded;

        /// <inheritdoc/>
        public event EventHandler<string>? OnRemoved;

        /// <inheritdoc/>
        public IReadOnlyList<Pipeline> GetAll()
        {
            var result = _pipelineDefinitions.Values.ToArray();
            return result;
        }

        /// <inheritdoc/>
        public void Create(Pipeline pipeline)
        {
            if (pipeline is null)
                throw new ArgumentNullException(nameof(pipeline));

            if (string.IsNullOrWhiteSpace(pipeline.Name))
                throw new ArgumentException("Pipeline must have a name.", nameof(pipeline));

            lock (_lock)
            {
                if (!_pipelineDefinitions.TryAdd(pipeline.Name, pipeline))
                {
                    throw new InvalidOperationException($"Pipeline '{pipeline.Name}' already exists.");
                }

                if (!_pipelineStateService.TryAdd(pipeline.Name))
                {
                    var rollbackSucceeded = _pipelineDefinitions.TryRemove(pipeline.Name, out _);

                    _logger.LogCritical(
                        "Invariant violation: pipeline '{Name}' was added to the registry, " +
                        "but PipelineStateService already had a state with the same name. " +
                        "Rollback {RollbackOutcome}. " +
                        "This indicates a leak in Remove() or a stale state.",
                        pipeline.Name,
                        rollbackSucceeded ? "succeeded" : "FAILED — the registry is now inconsistent");

                    throw new InvalidOperationException(
                        $"Pipeline '{pipeline.Name}' could not be registered due to an internal state conflict.");
                }
            }

            // Событие намеренно вызывается ВНЕ lock (см. remarks у OnAdded):
            // - медленный/блокирующий подписчик не тормозит Create/Remove;
            // - подписчик, дернувший реестр повторно, не получит deadlock.
            // Плата: при конкурентных Create/Remove одного имени порядок
            // OnAdded/OnRemoved не гарантирован. Не переносить вызов внутрь lock,
            // не прочитав предупреждение.
            try
            {
                OnAdded?.Invoke(this, pipeline);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Pipeline '{Name}' was successfully registered, but a subscriber of OnAdded threw an exception. " +
                    "The pipeline will still be scheduled.",
                    pipeline.Name);
            }
        }

        /// <inheritdoc/>
        public bool Remove(string pipelineName)
        {
            if (string.IsNullOrWhiteSpace(pipelineName))
            {
                throw new ArgumentException("Pipeline name is required.", nameof(pipelineName));
            }

            bool removed = false;
            lock (_lock)
            {
                if (_pipelineStateService.TryRemove(pipelineName))
                {
                    removed = _pipelineDefinitions.TryRemove(pipelineName, out _);
                    if (!removed)
                    {
                        _logger.LogCritical(
                            "Invariant violation: state for '{Name}' removed, but definition is missing. " +
                            "The pipeline is now orphaned in the registry.",
                            pipelineName);
                    }
                }
            }

            if (removed)
            {
                // См. комментарий у OnAdded: событие вне lock, порядок с OnAdded
                // при конкурентных Create/Remove не гарантирован. Не переносить внутрь lock.
                try
                {
                    OnRemoved?.Invoke(this, pipelineName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Pipeline '{Name}' was successfully removed, but a subscriber of OnRemoved threw an exception.",
                        pipelineName);
                }
            }          
            return removed;
        }

        /// <inheritdoc/>
        public bool Exist(string pipelineName)
        {
            if (string.IsNullOrWhiteSpace(pipelineName))
            {
                throw new ArgumentException("Pipeline name is required.", nameof(pipelineName));
            }

            var result = _pipelineDefinitions.ContainsKey(pipelineName);
            
            return result;
        }
    }
}
