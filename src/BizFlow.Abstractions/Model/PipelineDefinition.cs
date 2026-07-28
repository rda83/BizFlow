namespace BizFlow.Abstractions.Model
{
    public class PipelineDefinition
    {
        public string Name { get; }
        //public IWorker Worker { get; }
        public ISchedule Schedule { get; }

        public PipelineDefinition(
            string name,
            ////IWorker worker, 
            ISchedule schedule)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            //Worker = worker ?? throw new ArgumentNullException(nameof(worker));
            Schedule = schedule ?? throw new ArgumentNullException(nameof(schedule));
        }
    }
}
