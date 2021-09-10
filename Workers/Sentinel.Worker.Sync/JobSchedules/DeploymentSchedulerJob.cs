using System.Threading.Tasks;
using Quartz;

namespace Sentinel.Worker.Sync.JobSchedules
{
    public class DeploymentSchedulerJob : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            // throw new System.NotImplementedException();
            return Task.CompletedTask;
        }
    }
}