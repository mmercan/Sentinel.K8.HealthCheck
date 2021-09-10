using System.Threading.Tasks;
using Quartz;

namespace Sentinel.Worker.Sync.JobSchedules
{
    public class ServiceSchedulerJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return Task.CompletedTask;
        }
    }
}