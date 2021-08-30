using System.Threading.Tasks;
using Quartz;

namespace Sentinel.Worker.Sync.JobSchedules
{
    public class NamespaceSchedulerJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            throw new System.NotImplementedException();
        }
    }
}