using System.Threading.Tasks;
using Quartz;
using Sentinel.Models.K8sDTOs;
using Sentinel.Scheduler;

namespace Sentinel.Worker.Scheduler.JobSchedules
{
    public class HealthCheckResourceFeederJob : IJob
    {
        private readonly List<ISchedulerRepositoryFeeder> repositories;
        private readonly SchedulerRedisRepositoryFeeder<HealthCheckResourceV1> _healthCheckResourceFeeder;
        private readonly ILogger<HealthCheckResourceFeederJob> _logger;

        public HealthCheckResourceFeederJob(
            SchedulerRedisRepositoryFeeder<HealthCheckResourceV1> healthCheckResourceFeeder,
            IConfiguration configuration, ILogger<HealthCheckResourceFeederJob> logger)
        {
            repositories = new List<ISchedulerRepositoryFeeder>();
            _healthCheckResourceFeeder = healthCheckResourceFeeder;
            _logger = logger;
            repositories.Add(_healthCheckResourceFeeder);
        }

        public Task Execute(IJobExecutionContext context)
        {
            return Task.Run(() =>
             {
                 foreach (var item in repositories)
                 {
                     try
                     {
                         item.Sync();
                     }
                     catch (Exception ex)
                     {
                         _logger.LogWarning("HealthCheckResourceFeederJob Failed on {CollectionName} with Error {error}",
                         item.GetType().Name, ex.Message);
                     }
                 }

             });
        }
    }
}