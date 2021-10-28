using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sentinel.Common;
using Sentinel.Models.K8sDTOs;
using Sentinel.Scheduler;
using TimeZoneConverter;

namespace Sentinel.Worker.Scheduler.Schedules
{
    public class BusScheduler : BackgroundServiceWithHealthCheck
    {

        private readonly IBus _bus;
        private readonly SchedulerRepository<HealthCheckResourceV1> _healthCheckRepository;

        public BusScheduler(
            ILogger<BusScheduler> logger,
            IBus bus,
            IOptions<HealthCheckServiceOptions> hcoptions,
            SchedulerRepository<HealthCheckResourceV1> healthCheckRepository
            ) : base(logger, hcoptions)
        {
            _bus = bus;
            _healthCheckRepository = healthCheckRepository;
        }

        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ExecuteOnceAsync(stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private Task ExecuteOnceAsync(CancellationToken stoppingToken)
        {
            var referenceTime = DateTime.UtcNow;

            TimeZoneInfo tzi = TZConvert.GetTimeZoneInfo("Australia/Melbourne");
            var localtime = TimeZoneInfo.ConvertTime(referenceTime, tzi);

            _logger.LogInformation("BusScheduler : Local time zone: " + tzi.DisplayName + " and Local Time is " + localtime.ToString());
            _logger.LogInformation("BusScheduler : Checking for HealthCheckRepository ScheduledTasks " + _healthCheckRepository.ScheduledTasks.Count.ToString() + " Counted");

            var tasksThatShouldRun = _healthCheckRepository.ScheduledTasks.Where(t => t.ShouldRun(localtime)).ToList();
            foreach (var taskThatShouldRun in tasksThatShouldRun)
            {
                taskThatShouldRun.Increment();
                _logger.LogCritical("BusScheduler : Task Adding to RabbitMQ " + taskThatShouldRun.Task.Key);
            }
            return Task.CompletedTask;
        }
    }
}