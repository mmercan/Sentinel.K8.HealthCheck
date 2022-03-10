using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sentinel.Common.AuthServices;
using Sentinel.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Common.Tests.AuthServices
{
    public class AZAuthServiceTests
    {
        private ITestOutputHelper output;
        private IConfiguration config;
        public AZAuthServiceTests(ITestOutputHelper output)
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
        }

        [Fact]
        public async void AuthenticateShouldRun()
        {
            ILogger<AZAuthService> logger = Helpers.GetLogger<AZAuthService>();
            IOptions<AZAuthServiceSettings> settingsOptions = Options.Create(
                new AZAuthServiceSettings { ClientId = config["AzureAd:ClientId"], Secret = config["AzureAd:Secret"], TenantId = config["AzureAd:TenantId"] });
            IMemoryCache memoryCache = new MemoryCache(new MemoryCacheOptions());


            var azAuthService = new AZAuthService(logger, settingsOptions, memoryCache);
            var token = await azAuthService.AuthenticateAsync();
            Assert.NotNull(token);

            var token_2 = await azAuthService.AuthenticateAsync();
            Assert.NotNull(token_2);
        }
    }
}