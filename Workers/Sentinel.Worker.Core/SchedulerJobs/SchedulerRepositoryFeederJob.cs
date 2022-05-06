using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using Sentinel.Models.K8sDTOs;
using Sentinel.Scheduler;


namespace Workers.Sentinel.Worker.Core.SchedulerJobs
{
    [QuartzJob(ConfigurationSection = "Schedules:SchedulerRepositoryFeederJob", Group = "SchedulerFeederJobs")]
    public class SchedulerRepositoryFeederJob : IJob
    {
        private readonly List<ISchedulerRepositoryFeeder> repositories;
        private readonly SchedulerRedisRepositoryFeeder<HealthCheckResourceV1> _healthCheckResourceFeeder;
        private readonly ILogger<SchedulerRepositoryFeederJob> _logger;

        public SchedulerRepositoryFeederJob(
            IConfiguration configuration, ILogger<SchedulerRepositoryFeederJob> logger,
            IServiceProvider serviceProvider, IServiceCollection services)
        {
            repositories = new List<ISchedulerRepositoryFeeder>();

            var feederTypes = services.Where(x => x.ServiceType.GetInterfaces().Contains(typeof(ISchedulerRepositoryFeeder))).ToList();

            foreach (var feederType in feederTypes)
            {
                var feed = serviceProvider.GetService(feederType.ServiceType) as ISchedulerRepositoryFeeder;
                if (feed != null)
                {
                    repositories.Add(feed);
                }
            }
            _logger = logger;
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