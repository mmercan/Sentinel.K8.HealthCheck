using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyNetQ;
using Polly;
using Polly.Retry;
using Quartz;
using Sentinel.Common;
using Sentinel.Models.Scheduler;
using Sentinel.Scheduler;
using Sentinel.Scheduler.GeneralScheduler;
using StackExchange.Redis;
using TimeZoneConverter;

namespace Scheduler.JobSchedules
{
    [QuartzJob(Name = "BusScheduler", Group = "Scheduler", CronExpression = "0 */1 * * * ?", Description = "BusScheduler Check Cron of All Scheduled Repositories and Ad them to RabbitMQ Queues")]
    public class BusScheduler : IJob
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<BusScheduler> _logger;
        private readonly IBus _bus;
        private readonly string timezone;
        List<IScheduledTask> _scheduledTasks;
        public BusScheduler(IConfiguration configuration, ILogger<BusScheduler> logger,
            IServiceProvider serviceProvider, IServiceCollection services, IBus bus)
        {
            _configuration = configuration;
            _logger = logger;
            _bus = bus;

            _scheduledTasks = new List<IScheduledTask>();

            if (!string.IsNullOrWhiteSpace(_configuration["timezone"]))
            {
                timezone = _configuration["timezone"];
            }
            else
            {
                timezone = "Australia/Melbourne";
            }



            var feederTypes = services.Where(x => x.ServiceType.GetInterfaces().Contains(typeof(ISchedulerRepository))).ToList();
            foreach (var item in feederTypes)
            {
                var res = serviceProvider.GetService(item.ServiceType) as ISchedulerRepository;
                if (res != null)
                {
                    var copy = res.IScheduledTasks;
                    _scheduledTasks.AddRange(res.IScheduledTasks);
                }
            }
        }
        public Task Execute(IJobExecutionContext context)
        {
            var referenceTime = DateTime.UtcNow;
            TimeZoneInfo tzi = TZConvert.GetTimeZoneInfo(timezone);

            var tasksThatShouldRun = _scheduledTasks.Where(t => t.ShouldRun(referenceTime, tzi)).ToList();

            _logger.LogTrace("BusScheduler : Checking for HealthCheckRepository ScheduledTasks {AllCount}  Counted {runCount} will be triggered",
             _scheduledTasks.Count.ToString(), tasksThatShouldRun.Count.ToString());

            foreach (var taskThatShouldRun in tasksThatShouldRun)
            {
                taskThatShouldRun.Increment();
                var type = taskThatShouldRun.IScheduledTaskItem.GetType();

                _logger.LogInformation("BusScheduler : Task Adding to RabbitMQ Key : {key} ", taskThatShouldRun.IScheduledTaskItem.Key);

                // TODO: Add a check to see if the service added to object before sending the message

                _bus.PubSub.PublishAsync(taskThatShouldRun.IScheduledTaskItem, _configuration["queue:healthcheck"]).ContinueWith(task =>
                 {
                     if (task.IsCompleted && !task.IsFaulted)
                     {
                         _logger.LogInformation("Task Added to RabbitMQ on topic {healthcheck} with Key {Key} {type}", _configuration["queue:healthcheck"], taskThatShouldRun.IScheduledTaskItem.Key, taskThatShouldRun.IScheduledTaskItem.GetType().Name);
                     }
                     if (task.IsFaulted)
                     {
                         _logger.LogError("BusScheduler Failed : {Exception} ", task.Exception.MessageWithInnerException());
                         var constring = _configuration["RabbitMQConnection"];
                         _logger.LogDebug("RabbitMQConnection {RabbitMQConnection}", constring);
                     }
                 });
            }
            return Task.CompletedTask;
        }
    }
}