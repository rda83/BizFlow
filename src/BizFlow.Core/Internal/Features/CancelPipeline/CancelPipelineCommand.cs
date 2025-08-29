
namespace BizFlow.Core.Internal.Features.CancelPipeline
{
    public class CancelPipelineCommand
    {
        public string PipelineName { get; set; } = string.Empty;
        public DateTime ExpirationTime { get; set; }
        public string? Description { get; set; } = string.Empty;
        public bool ClosingByExpirationTimeOnly { get; set; }
    }
}
