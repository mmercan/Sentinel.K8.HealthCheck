using Sentinel.Models.K8sDTOs;
using Sentinel.Tests.Helpers;
using StackExchange.Redis;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Scheduler.Tests
{
    public class SchedulerRedisRepositoryFeederTests
    {
        private readonly ITestOutputHelper _output;

        public SchedulerRedisRepositoryFeederTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void SchedulerRepositoryFeederShuldInitiate()
        {



            var loggerrepo = Sentinel.Tests.Helpers.Helpers.GetLogger<SchedulerRepository<HealthCheckResourceV1>>();

            var loggerfeeder = Sentinel.Tests.Helpers.Helpers.GetLogger<SchedulerRedisRepositoryFeeder<HealthCheckResourceV1>>();
            var repo = new SchedulerRepository<HealthCheckResourceV1>(loggerrepo);
            IConnectionMultiplexer rediscon = RedisExtensions.GetRedisMultiplexer();

            // var hc = new HealthCheckResourceV1();
            // hc.Schedule = "* * * * *";
            // hc.Name = "test";
            // hc.Namespace = "default";

            // var feeder = new SchedulerRedisRepositoryFeeder<HealthCheckResourceV1>(repo, loggerfeeder, rediscon);
            // feeder.Initiate("HealthChecks");
            // feeder.Sync();

            //repo.Items.Add(hc);

            //repo.UpdateItem(hc);

            // repo.Items.Remove(hc);

            // _output.WriteLine("Hello World!");
        }



    }
}