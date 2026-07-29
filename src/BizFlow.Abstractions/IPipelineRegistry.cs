using BizFlow.Abstractions.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
