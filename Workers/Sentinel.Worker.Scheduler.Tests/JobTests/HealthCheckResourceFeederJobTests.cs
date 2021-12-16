using System.Collections.Generic;
using System.Threading;
using Microsoft.Extensions.Configuration;
using Moq;
using Quartz;
using Sentinel.Models.K8sDTOs;
using Sentinel.Scheduler;
using Sentinel.Tests.Helpers;
using Sentinel.Worker.Scheduler.JobSchedules;
using StackExchange.Redis;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Worker.Scheduler.Tests.JobTests
{
    public class HealthCheckResourceFeederJobTests
    {
        private readonly ITestOutputHelper _output;
        private readonly IConfiguration config;
        public HealthCheckResourceFeederJobTests(ITestOutputHelper output)
        {
            _output = output;

            var myConfiguration = new Dictionary<string, string>
            {
                {"Rediskey:HealthChecks", "HealthChecks"},

            };


            config = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();
        }

        [Fact]
        public void SchedulerRepositoryFeederTests()
        {



            var loggerrepo = Sentinel.Tests.Helpers.Helpers.GetLogger<SchedulerRepository<HealthCheckResourceV1>>();

            var loggerfeeder = Sentinel.Tests.Helpers.Helpers.GetLogger<SchedulerRepositoryFeeder<HealthCheckResourceV1>>();

            var repo = new SchedulerRepository<HealthCheckResourceV1>(loggerrepo);
            IConnectionMultiplexer rediscon = RedisExtensions.GetRedisMultiplexer();



            var feeder = new SchedulerRepositoryFeeder<HealthCheckResourceV1>(repo, loggerfeeder, rediscon);
            feeder.Initiate("HealthChecks");
            feeder.Sync();

            HealthCheckResourceFeederJob job = new HealthCheckResourceFeederJob(feeder, config);


            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(20 * 1000);

            var contextMoc = new Mock<IJobExecutionContext>();
            contextMoc.Setup(m => m.CancellationToken).Returns(source.Token);

            var jobtask = job.Execute(contextMoc.Object);

            try
            {
                jobtask.Wait(source.Token);
            }
            catch { }

        }
    }
}