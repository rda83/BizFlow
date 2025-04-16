
namespace BizFlow.Core
{
    public class Pipeline
    {
        public string Name { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;




        public List<string> PipelineItems = new List<string>();
            //public TypeOperation TypeOperation { get; set; }
            //public int SortOrder { get; set; }
            //public string Description { get; set; }
            //public bool Blocked { get; set; }
    }
}
