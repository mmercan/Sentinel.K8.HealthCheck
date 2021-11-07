using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Configuration;
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

        private readonly EasyNetQ.IBus _bus;
        private readonly SchedulerRepository<HealthCheckResourceV1> _healthCheckRepository;
        private readonly IConfiguration _configuration;

        public BusScheduler(
            ILogger<BusScheduler> logger,
            IBus bus,
            IOptions<HealthCheckServiceOptions> hcoptions,
            SchedulerRepository<HealthCheckResourceV1> healthCheckRepository,
            IConfiguration configuration
            ) : base(logger, hcoptions)
        {
            _bus = bus;
            _healthCheckRepository = healthCheckRepository;
            _configuration = configuration;
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

            var tasksThatShouldRun = _healthCheckRepository.ScheduledTasks.Where(t => t.ShouldRun(referenceTime, tzi)).ToList();

            _logger.LogTrace("BusScheduler : Checking for HealthCheckRepository ScheduledTasks " + _healthCheckRepository.ScheduledTasks.Count.ToString() + " Counted " +
            tasksThatShouldRun.Count.ToString() + " will be triggered");

            foreach (var taskThatShouldRun in tasksThatShouldRun)
            {
                taskThatShouldRun.Increment();
                _logger.LogInformation("BusScheduler : Task Adding to RabbitMQ " + taskThatShouldRun.Task.Key);

                _bus.PubSub.PublishAsync(taskThatShouldRun.Item, _configuration["queue:healthcheck"]).ContinueWith(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted)
                    {
                        _logger.LogInformation("Task Added to RabbitMQ " + _configuration["queue:healthcheck"] + " " + taskThatShouldRun.Task.Key);
                    }
                    if (task.IsFaulted)
                    {
                        _logger.LogError(task.Exception.Message);
                        // _bus.
                        var constring = _configuration["RabbitMQConnection"];
                        _logger.LogDebug(constring);

                    }
                });

            }

            return Task.CompletedTask;
        }
    }
}