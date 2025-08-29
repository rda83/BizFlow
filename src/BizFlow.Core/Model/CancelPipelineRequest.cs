
namespace BizFlow.Core.Model
{
    public class CancelPipelineRequest
    {
        public string PipelineName { get; set; } = string.Empty;
        public DateTime ExpirationTime { get; set; }
        public string? Description { get; set; } = string.Empty;
        public bool ClosingByExpirationTimeOnly { get; set; }
        public DateTime Created { get; set; }
    }
}
