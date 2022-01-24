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
using Sentinel.Redis;
using Sentinel.Scheduler;
using StackExchange.Redis;
using TimeZoneConverter;
using Sentinel.Scheduler.Helpers;
using Polly;
using Sentinel.Models.CRDs;
using Polly.Retry;

namespace Sentinel.Worker.Scheduler.Schedules
{
    public class BusScheduler : BackgroundServiceWithHealthCheck
    {

       
        
       
        private readonly EasyNetQ.IBus _bus;
        private readonly SchedulerRepository<HealthCheckResourceV1> _healthCheckRepository;
        private readonly IConfiguration _configuration;
        private readonly RedisDictionary<ServiceV1> redisServiceDictionary;
        private readonly RedisDictionary<HealthCheckResourceV1> redisHealCheckServiceNotFoundDictionary;
        private readonly string timezone;
        protected readonly RetryPolicy policy;
        public BusScheduler(
            ILogger<BusScheduler> logger,
            IBus bus,
            IOptions<HealthCheckServiceOptions> hcoptions,
            SchedulerRepository<HealthCheckResourceV1> healthCheckRepository,
            IConnectionMultiplexer _multiplexer,
            IConfiguration configuration
            ) : base(logger, hcoptions)
        {
            _bus = bus;
            _healthCheckRepository = healthCheckRepository;
            _configuration = configuration;

            redisServiceDictionary = new RedisDictionary<ServiceV1>(_multiplexer, _logger, configuration["Rediskey:Services"]);
            redisHealCheckServiceNotFoundDictionary = new RedisDictionary<HealthCheckResourceV1>(_multiplexer, _logger, configuration["Rediskey:HealCheckServiceNotFound"]);

            if (!string.IsNullOrWhiteSpace(_configuration["timezone"]))
            {
                timezone = _configuration["timezone"];
            }
            else
            {
                timezone = "Australia/Melbourne";
            }

            policy = Policy.Handle<RedisTimeoutException>().Or<RedisConnectionException>().WaitAndRetry(new[]
              {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
              }, (ex, timeSpan, retryCount, context) =>
              {
                  _logger.LogError(ex, "BusScheduler : Polly retry " + retryCount.ToString() + "  Error Finding Service Related to HealthCheckResourceV1");
                  if (retryCount == 2)
                  {
                      throw ex;
                  }
                  // Add logic to be executed before each retry, such as logging    
              });

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
            TimeZoneInfo tzi = TZConvert.GetTimeZoneInfo(timezone);

            var tasksThatShouldRun = _healthCheckRepository.ScheduledTasks.Where(t => t.ShouldRun(referenceTime, tzi)).ToList();

            _logger.LogTrace("BusScheduler : Checking for HealthCheckRepository ScheduledTasks " + _healthCheckRepository.ScheduledTasks.Count.ToString() + " Counted " + tasksThatShouldRun.Count.ToString() + " will be triggered");

            foreach (var taskThatShouldRun in tasksThatShouldRun)
            {
                taskThatShouldRun.Increment();
                _logger.LogInformation("BusScheduler : Task Adding to RabbitMQ " + taskThatShouldRun.Task.Key);

                try
                {
                    policy.Execute(() =>
                    {
                        var service = taskThatShouldRun.Item.FindServiceRelatedtoHealthCheckResourceV1(redisServiceDictionary);
                        taskThatShouldRun.Item.RelatedService = service;
                        if (service == null)
                        {
                            _logger.LogCritical("BusScheduler : Error Finding Service Related to HealthCheckResourceV1 Logged in RedisHealCheckServiceNotFoundDictionary");
                            redisHealCheckServiceNotFoundDictionary.Add(taskThatShouldRun.Item);
                        }
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "BusScheduler : Error Finding Service Related to HealthCheckResourceV1");
                }

                // TODO: Add a check to see if the service added to object before sending the message
                _bus.PubSub.PublishAsync(taskThatShouldRun.Item, _configuration["queue:healthcheck"]).ContinueWith(task =>
                {
                    if (task.IsCompleted && !task.IsFaulted)
                    {
                        _logger.LogInformation("Task Added to RabbitMQ " + _configuration["queue:healthcheck"] + " " + taskThatShouldRun.Task.Key);
                    }
                    if (task.IsFaulted)
                    {
                        _logger.LogError("BusScheduler Failed : " + task.Exception.MessageWithInnerException());
                        var constring = _configuration["RabbitMQConnection"];
                        _logger.LogDebug(constring);
                    }
                });

            }

            return Task.CompletedTask;
        }

    }
}