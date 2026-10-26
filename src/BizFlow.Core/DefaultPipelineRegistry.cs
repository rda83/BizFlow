using BizFlow.Abstractions;
using BizFlow.Abstractions.Model;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace BizFlow.Core
{
    public class DefaultPipelineRegistry : IPipelineRegistry
    {
        private readonly ILogger<DefaultPipelineRegistry> _logger;
        private readonly PipelineStateService _pipelineStateService;
        private readonly ConcurrentDictionary<string, Pipeline> _pipelineDefinitions = new();
        private readonly object _lock = new object();

        public DefaultPipelineRegistry(
            PipelineStateService pipelineStateService,
            ILogger<DefaultPipelineRegistry> logger)
        {
            _pipelineStateService = pipelineStateService;
            _logger = logger;
        }

        public event EventHandler<Pipeline>? OnAdded;
        public event EventHandler<string>? OnRemoved;

        public IReadOnlyList<Pipeline> GetAll()
        {
            var result = _pipelineDefinitions.Values.ToArray();
            return result;
        }

        public void Create(Pipeline pipeline)
        {
            if (pipeline is null || string.IsNullOrWhiteSpace(pipeline.Name))
            {
                throw new ArgumentException("Pipeline is required and must have a name.", nameof(pipeline));
            }

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
                        "Invariant violation: pipeline '{Name}' was added to the registry, " +  // LOG-OK
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
                    "Pipeline '{Name}' was successfully registered, but a subscriber of OnAdded threw an exception. " +  // LOG-OK
                    "The pipeline will still be scheduled.",
                    pipeline.Name);
            }
        }

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
