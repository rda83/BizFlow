
namespace BizFlow.Abstractions
{
    public class JobDefinition
    {
        public string Name { get; }
        public IWorker Worker { get; }
        public ISchedule Schedule { get; }

        public JobDefinition(string name, IWorker worker, ISchedule schedule)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Worker = worker ?? throw new ArgumentNullException(nameof(worker));
            Schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
        }
    }
}
