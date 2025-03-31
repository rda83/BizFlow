
namespace BizFlow.Core.Internal
{
    public interface IJobsManager
    {
        void CrerateJob(string name, string cronExpression);
    }
}
