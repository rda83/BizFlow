
namespace BizFlow.Core
{
    public class Pipeline
    {
        public string Name { get; set; } = string.Empty;
        public string CronExpression { get; set; } = string.Empty;

        //Указываем тип задачи, в соответствии с ней будет найден воркер
        //      BizFlowWorker 1
    }
}
