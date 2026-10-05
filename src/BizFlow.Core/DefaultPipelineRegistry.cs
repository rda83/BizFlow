using BizFlow.Abstractions;
using BizFlow.Abstractions.Model;
using System.Collections.Concurrent;

namespace BizFlow.Core
{
    public class DefaultPipelineRegistry : IPipelineRegistry
    {
        private readonly PipelineStateService _pipelineStateService;
        private readonly ConcurrentDictionary<string, Pipeline> _pipelineDefinitions = new();
        private readonly object _lock = new object();

        public DefaultPipelineRegistry(PipelineStateService pipelineStateService)
        {
            _pipelineStateService = pipelineStateService;
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
            if (string.IsNullOrEmpty(pipeline.Name))
            {
                throw new ArgumentException("Pipeline name is required.", nameof(pipeline));
            }

            bool added = false;
            lock (_lock)
            {
                added = _pipelineDefinitions.TryAdd(pipeline.Name, pipeline);
            }

            if (added)
            {
                if (_pipelineStateService.TryAdd(pipeline.Name))
                {
                    OnAdded?.Invoke(this, pipeline);
                }
                else
                {
                    throw new InvalidOperationException($"Pipeline '{pipeline.Name}' ****."); // нужно подумать над сообщением
                }
            }
            else
            {
                throw new InvalidOperationException($"Pipeline '{pipeline.Name}' already exists.");
            }
        }

        public bool Remove(string pipelineName)
        {
            if (string.IsNullOrEmpty(pipelineName))
            {
                throw new ArgumentException("Pipeline name is required.", nameof(pipelineName));
            }

            var removed = false;
            if (_pipelineStateService.TryRemove(pipelineName))
            {
                lock (_lock) 
                {
                    if(_pipelineDefinitions.TryRemove(pipelineName, out var _))
                    {
                        removed = true;
                    }
                }
            }

            if (removed)
            {
                OnRemoved?.Invoke(this, pipelineName);
            }          
            return removed;
        }

        public bool Exist(string pipelineName)
        {
            if (string.IsNullOrEmpty(pipelineName))
            {
                throw new ArgumentException("Pipeline name is required.", nameof(pipelineName));
            }

            var result = _pipelineDefinitions.ContainsKey(pipelineName);
            
            return result;
        }
    }
}
