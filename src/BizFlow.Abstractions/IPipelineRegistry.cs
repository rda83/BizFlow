using BizFlow.Abstractions.Model;

namespace BizFlow.Abstractions
{
    public interface IPipelineRegistry
    {
        IReadOnlyList<Pipeline> GetAll();
        void Create(Pipeline pipeline);
        bool Remove(string pipelineName);
        bool Exist(string pipelineName);

        event EventHandler<Pipeline>? OnAdded;
        event EventHandler<string>? OnRemoved;
    }
}
