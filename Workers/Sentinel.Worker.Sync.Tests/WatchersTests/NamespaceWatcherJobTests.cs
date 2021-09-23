using System.Threading;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentinel.K8s;
using Sentinel.Worker.Sync.TestsHelpers;
using Sentinel.Worker.Sync.Watchers;
using StackExchange.Redis;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Worker.Sync.Tests.WatchersTests
{
    public class NamespaceWatcherJobTests
    {
        private readonly ITestOutputHelper _output;

        public NamespaceWatcherJobTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Fact]
        public void NamespaceWatcherJobShouldRun()
        {

            var client = KubernetesClientTestHelper.GetKubernetesClient();
            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<NamespaceWatcherJob>();

            var config = new MapperConfiguration(cfg =>
              {
                  cfg.AddProfile(new K8SMapper());
              });
            var mapper = config.CreateMapper();

            IConnectionMultiplexer rediscon = ConnectionMultiplexer.Connect("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc");


            var builder = new ConfigurationBuilder()
                       // .SetBasePath(env.ContentRootPath)
                       // .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                       //.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                       .AddEnvironmentVariables();
            var configuration = builder.Build();

            IServiceCollection serviceCollection = new ServiceCollection();
            var provider = serviceCollection.BuildServiceProvider();
            // .UseTextServices()
            // .BuildServiceProvider();



            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(3 * 1000);

            NamespaceWatcherJob service = new NamespaceWatcherJob(provider, logger, serviceCollection, client);
            service.StartAsync(source.Token);
        }

    }
}