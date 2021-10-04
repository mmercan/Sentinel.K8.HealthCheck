using System.Threading;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            var maper = GetIMapperExtension.GetIMapper(cfg => cfg.AddProfile(new K8SMapper()));
            IConnectionMultiplexer rediscon = RedisExtensions.GetRedisMultiplexer();


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
            source.CancelAfter(40 * 1000);

            var hcoptions = Options.Create(new HealthCheckServiceOptions());

            NamespaceWatcherJob service = new NamespaceWatcherJob(logger, client, rediscon, maper, hcoptions);
            service.StartAsync(source.Token);
        }

    }
}