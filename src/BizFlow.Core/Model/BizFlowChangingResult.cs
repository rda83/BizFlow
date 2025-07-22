
namespace BizFlow.Core.Model
{
    public class BizFlowChangingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string LaunchId { get; set; }
        public List<CheckItemsError> CheckItemsErrors { get; set; }
            = new List<CheckItemsError>();
    }
}
