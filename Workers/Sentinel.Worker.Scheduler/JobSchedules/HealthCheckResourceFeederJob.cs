using System.Threading.Tasks;
using Quartz;
using Sentinel.Models.K8sDTOs;
using Sentinel.Scheduler;

namespace Sentinel.Worker.Scheduler.JobSchedules
{
    public class HealthCheckResourceFeederJob : IJob
    {
        private readonly SchedulerRepositoryFeeder<HealthCheckResourceV1> _healthCheckResourceFeeder;

        public HealthCheckResourceFeederJob(SchedulerRepositoryFeeder<HealthCheckResourceV1> healthCheckResourceFeeder)
        {
            _healthCheckResourceFeeder = healthCheckResourceFeeder;
            _healthCheckResourceFeeder.Initiate("HealthChecks");
        }

        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(() =>
             {
                 _healthCheckResourceFeeder.Sync();
             });
        }
    }
}