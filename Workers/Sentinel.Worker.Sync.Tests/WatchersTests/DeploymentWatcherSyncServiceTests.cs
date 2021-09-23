using System.Threading;
using AutoMapper;
using Sentinel.K8s;
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
            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<DeploymentWatcherSyncService>();

            var config = new MapperConfiguration(cfg =>
              {
                  cfg.AddProfile(new K8SMapper());
              });
            var mapper = config.CreateMapper();

            IConnectionMultiplexer rediscon = ConnectionMultiplexer.Connect("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc");

            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(3 * 1000);

            DeploymentWatcherSyncService service = new DeploymentWatcherSyncService(logger, client, rediscon);
            service.StartAsync(source.Token);
        }

    }
}