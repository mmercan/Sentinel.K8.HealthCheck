using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sentinel.Common.AuthServices;
using Sentinel.Common.HttpClientServices;
using Sentinel.Models.K8sDTOs;
using Sentinel.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Common.Tests.HttpClientServices
{
    public class IsAliveAndWellHealthCheckDownloaderTests
    {
        private readonly ITestOutputHelper output;
        private readonly IConfiguration config;
        private readonly HttpClient client;
        private readonly IOptions<AZAuthServiceSettings> settingsOptions;
        private readonly AZAuthService azAuthService;
        private readonly IsAliveAndWellHealthCheckDownloader downloader;
        public IsAliveAndWellHealthCheckDownloaderTests(ITestOutputHelper output)
        {
            this.output = output;

            var myConfiguration = new Dictionary<string, string>
            {
                {"RequiredHeaders:0", "Internal"},
                {"RequiredHeaders:1", "External"},
            };


            config = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .AddEnvironmentVariables()
            .Build();

            client = new HttpClient();
            ILogger<IsAliveAndWellHealthCheckDownloader> logger = Helpers.GetLogger<IsAliveAndWellHealthCheckDownloader>();


            ILogger<AZAuthService> loggerAzService = Helpers.GetLogger<AZAuthService>();
            settingsOptions = Options.Create(
                new AZAuthServiceSettings { ClientId = config["AzureAd:ClientId"], Secret = config["AzureAd:Secret"], TenantId = config["AzureAd:TenantId"] });
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());

            azAuthService = new AZAuthService(loggerAzService, settingsOptions, memoryCache);

            downloader = new IsAliveAndWellHealthCheckDownloader(client, logger, config, azAuthService);
        }

        [Fact]
        public void TestName()
        {
            // // Given
            // // download textfile for test
            var text = File.ReadAllText("../../../sentinel-dev-health-ui-app-health-ui.sentinel-dev.txt");
            var serv = text.FromJSON<ServiceV1>();
            // // When

            // // Then
            // Assert.NotEmpty(serv?.Name);

            //            var downTask = downloader.DownloadAsync(serv);
            // downTask.Wait(TimeSpan.FromSeconds(50));
            // Assert.NotEmpty(downTask.Result);
            Assert.NotNull(serv);
        }

    }
}