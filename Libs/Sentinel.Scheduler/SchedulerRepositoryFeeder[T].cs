using System.Threading.Tasks;
using Quartz;

namespace Sentinel.Scheduler
{
    public class SchedulerRepositoryFeeder<T> : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}