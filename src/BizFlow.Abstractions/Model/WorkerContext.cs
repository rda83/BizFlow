using System.Text.Json;

namespace BizFlow.Abstractions.Model
{
    public class WorkerContext
    {
        public string LaunchId { get; set; }
        public string TypeOperationId { get; set; }
        public string PipelineName { get; set; }
        public JsonElement Options { get; set; }
        public bool IsStartNowPipeline { get; set; }
    }
}
