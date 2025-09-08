
namespace BizFlow.Core.Internal.Features.CancelPipeline
{
    public class CloseCancelPipelineCommand
    {
        public long CancelPipeRequestId { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
