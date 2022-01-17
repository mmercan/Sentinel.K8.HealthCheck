using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Sentinel.Common.AuthServices;
using Sentinel.Common.HttpClientServices;
using Sentinel.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Common.Tests.HttpClientServices
{
    public class DownloadJsonServiceTests
    {
        private ITestOutputHelper output;
        private IConfiguration config;

        private AZAuthService azAuthService;
        private HttpClient client;
        private readonly IOptions<AZAuthServiceSettings> settingsOptions;
        private DownloadJsonService downloadJsonService;
        public DownloadJsonServiceTests(ITestOutputHelper output)
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


            ILogger<AZAuthService> logger = Helpers.GetLogger<AZAuthService>();
            settingsOptions = Options.Create(
                new AZAuthServiceSettings { ClientId = config["AzureAd:ClientId"], Secret = config["AzureAd:Secret"], TenantId = config["AzureAd:TenantId"] });
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());


            azAuthService = new AZAuthService(logger, settingsOptions, memoryCache);
            ILogger<DownloadJsonService> jsonlogger = Helpers.GetLogger<DownloadJsonService>();

            client = new HttpClient();
            downloadJsonService = new DownloadJsonService(client, jsonlogger, config, azAuthService);
        }


        [Fact]
        public void downloadJsonServiceShouldGetJson()
        {

            // Given
            var task = downloadJsonService.DownloadAsync(new Uri("https://httpbin.org/json"), settingsOptions.Value);
            task.Wait();


            // When
            var contentTask = task.Result.Content.ReadAsStringAsync();
            contentTask.Wait();
            var content = contentTask.Result.FromJSON<SlideShowContainer>();

            // Then
            Assert.NotEmpty(content.SlideShow.Title);
        }

        [Fact]
        public void downloadJsonServiceCanHandleNonSuccessCode()
        {
            // Given
            var task = downloadJsonService.DownloadAsync(new Uri("https://httpbin.org/status/401"), settingsOptions.Value);
            task.Wait();


            // When


            // Then
            Assert.Equal(task.Result.StatusCode, System.Net.HttpStatusCode.Unauthorized);
        }


    }

    public class SlideShowContainer
    {
        [JsonProperty(PropertyName = "slideshow")]
        public SlideShow SlideShow { get; set; } = default!;
    }

    public class SlideShow
    {
        public string Title { get; set; } = default!;
        public string Author { get; set; } = default!;

        public string Date { get; set; } = default!;

        public List<Slides> Slides { get; set; } = default!;
    }
    public class Slides
    {
        public string Title { get; set; } = default!;

        public string Type { get; set; } = default!;
        public List<string> Items { get; set; } = default!;
    }
}