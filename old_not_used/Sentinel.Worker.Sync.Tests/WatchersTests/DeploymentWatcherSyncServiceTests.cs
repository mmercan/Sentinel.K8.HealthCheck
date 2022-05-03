using System.Threading;
using AutoMapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Sentinel.K8s;
using Sentinel.Tests.Helpers;
using Sentinel.Worker.Sync.TestsHelpers;
using Sentinel.Worker.Sync.Watchers;
using StackExchange.Redis;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Worker.Sync.Tests.WatchersTests
{
    public class DeploymentWatcherSyncServiceTests
    {
        private readonly ITestOutputHelper _output;

        public DeploymentWatcherSyncServiceTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Fact]
        public void DeploymentWatcherSyncServiceShouldRun()
        {

            var client = KubernetesClientTestHelper.GetKubernetesClient();
            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<DeploymentWatcherJob>();

            var config = new MapperConfiguration(cfg =>
              {
                  cfg.AddProfile(new K8SMapper());
              });
            var mapper = config.CreateMapper();

            var maper = GetIMapperExtension.GetIMapper(cfg => cfg.AddProfile(new K8SMapper()));
            IConnectionMultiplexer rediscon = RedisExtensions.GetRedisMultiplexer();

            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(3 * 1000);

            var hcoptions = Options.Create(new HealthCheckServiceOptions());

            DeploymentWatcherJob service = new DeploymentWatcherJob(logger, client, rediscon, maper, hcoptions);
            service.StartAsync(source.Token);
        }

    }
}