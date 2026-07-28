using BizFlow.Abstractions;
using BizFlow.Abstractions.Model;
using System.Collections.Concurrent;

namespace BizFlow.Core
{
    public class DefaultBizFlowPipelineRegistry : IBizFlowPipelineRegistry
    {
        private readonly ConcurrentDictionary<string, PipelineDefinition> _pipelineDefinitions = new();

        public event EventHandler<PipelineDefinition>? PipelineAdded;
        public event EventHandler<string>? PipelineRemoved;

        public IReadOnlyList<PipelineDefinition> GetAll()
        {
            var result = _pipelineDefinitions.Values.ToArray();
            return result;
        }

        public void Create(PipelineDefinition pipeline)
        {
            if (string.IsNullOrEmpty(pipeline.Name))
            {
                throw new ArgumentException("Pipeline name is required.", nameof(pipeline));
            }

            var added = _pipelineDefinitions.TryAdd(pipeline.Name, pipeline);

            if (!added)
            {
                throw new InvalidOperationException($"Pipeline '{pipeline.Name}' already exists.");
            }

            PipelineAdded?.Invoke(this, pipeline);
        }

        public bool Remove(string pipelineName)
        {
            if (string.IsNullOrEmpty(pipelineName))
            {
                throw new ArgumentException("Pipeline name is required.", nameof(pipelineName));
            }

            var removed = _pipelineDefinitions.TryRemove(pipelineName, out  var _);
            if (removed)
            {
                PipelineRemoved?.Invoke(this, pipelineName);
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
