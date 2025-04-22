
namespace BizFlow.Core.Model
{
    public class PipelineItem
    {
        public string TypeOperationId { get; set; }
        public int SortOrder { get; set; }
        public string Description { get; set; }
        public bool Blocked { get; set; }
    }
}
