using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using Sentinel.Models.K8sDTOs;
using Sentinel.Scheduler;
using Sentinel.Tests.Helpers;
using Sentinel.Worker.Scheduler.Schedules;
using StackExchange.Redis;
using Xunit;
using Xunit.Abstractions;


namespace Sentinel.Worker.Scheduler.Tests.Schedules
{
    public class BusSchedulerTests
    {
        private readonly ITestOutputHelper _output;
        private readonly IConfiguration config;
        private readonly BusScheduler busScheduler;

        public BusSchedulerTests(ITestOutputHelper output)
        {
            _output = output;

            var bus = Substitute.For<EasyNetQ.IBus>();
            bus.PubSub.PublishAsync<HealthCheckResourceV1>(Arg.Any<HealthCheckResourceV1>(), "healthcheck").Returns(Task.CompletedTask);


            ILogger<BusScheduler> logger = Sentinel.Tests.Helpers.Helpers.GetLogger<BusScheduler>();

            IOptions<HealthCheckServiceOptions> hcoptions = Options.Create(
                new HealthCheckServiceOptions { }); ;
            SchedulerRepository<HealthCheckResourceV1> healthCheckRepository = CreateNewRepo();
            IConnectionMultiplexer _multiplexer = RedisExtensions.GetRedisMultiplexer(); ;

            var myConfiguration = new Dictionary<string, string>
            {
                {"Rediskey:HealthChecks", "HealthChecks"},
                { "Rediskey:Services", "Services"},
                {"Rediskey:HealCheckServiceNotFound", "HealCheckServiceNotFound"},
                {"queue:healthcheck", "healthcheck"}

            };

            config = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();

            busScheduler = new BusScheduler(logger, bus, hcoptions, healthCheckRepository, _multiplexer, config);


        }


        [Fact]
        public void TestName()
        {
            // Given busScheduler is created

            // When 

            // Then
            busScheduler.StartAsync(CancellationToken.None).Wait(TimeSpan.FromSeconds(65));
            // Then
            busScheduler.StopAsync(CancellationToken.None).Wait();
            Assert.True(true);
        }

        private SchedulerRepository<HealthCheckResourceV1> CreateNewRepo()
        {
            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<SchedulerRepository<HealthCheckResourceV1>>();
            var repo = new SchedulerRepository<HealthCheckResourceV1>(logger);

            var hc = new HealthCheckResourceV1();
            hc.Schedule = "* * * * *";
            hc.Name = "test";
            hc.Namespace = "default";
            hc.Spec = new HealthCheckResourceSpecV1 { Service = "kubernetes" };

            repo.Items.Add(hc);

            repo.UpdateItem(hc);

            // repo.Items.Remove(hc);
            return repo;
        }
    }
}