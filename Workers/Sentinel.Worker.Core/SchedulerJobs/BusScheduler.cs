using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using Sentinel.Models.Scheduler;
using Sentinel.Scheduler;

namespace Scheduler.JobSchedules
{
    [QuartzJob(Name = "BusScheduler", Description = "BusScheduler Check Cron of All Scheduled Repositories and Ad them to RabbitMQ Queues", Group = "Scheduler", CronExpression = "0 */1 * * * ?")]
    public class BusScheduler : IJob
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BusScheduler> _logger;

        public BusScheduler(IConfiguration configuration, ILogger<BusScheduler> logger,
            IServiceProvider serviceProvider, IServiceCollection services)
        {
            _configuration = configuration;
            _logger = logger;

            var repositories = new List<SchedulerRepository<IScheduledTaskItem>>();
            var feederTypes = services.Where(x => x.ServiceType.IsGenericType && x.ServiceType.GetGenericTypeDefinition() == typeof(SchedulerRepository<>)).ToList();
            foreach (var item in feederTypes)
            {
                //serviceProvider.GetService(item.ServiceType) 
                var res = serviceProvider.GetService(item.ServiceType) as ISchedulerRepository;
                if (res != null)
                {
                    var copy = res.ScheduledTaskCopy;
                    //   repositories.Add(res);
                }
                // Sentinel.Scheduler.SchedulerRepository<Sentinel.Models.K8sDTOs.HealthCheckResourceV1
            }
        }
        public Task Execute(IJobExecutionContext context)
        {
            return Task.CompletedTask;
            //throw new NotImplementedException();
        }
    }
}