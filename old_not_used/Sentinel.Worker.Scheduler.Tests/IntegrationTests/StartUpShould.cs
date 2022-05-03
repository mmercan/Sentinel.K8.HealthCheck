using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentinel.Tests.Helpers;
using Sentinel.Worker.Scheduler.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Worker.Scheduler.Tests.IntegrationTests
{

    [Collection("WebApplicationFactory")]
    public class StartUpShould
    {

        private WebApplicationFactory<Startup> factory;
        AuthTokenFixture authTokenFixture;
        private ITestOutputHelper _output;

        public StartUpShould(CustomWebApplicationFactory factory, AuthTokenFixture authTokenFixture, ITestOutputHelper output)
        {
            this.factory = factory;

            _output = output;
            this.authTokenFixture = authTokenFixture;
        }


        [Theory]
        [InlineData("/")]
        // [InlineData("/Health/IsAlive")]
        // [InlineData("/Health/IsAliveAndWell")]
        public void Run(string url)
        {
            var client = factory.CreateClient();
            // client.DefaultRequestHeaders.Add("api-version", "1.0"); client.DefaultRequestHeaders.Add("Authorization", this.authTokenFixture.Token);
            client.DefaultRequestHeaders.Add("Internal", "true");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            // Act
            var responseTask = client.GetAsync(url);
            responseTask.Wait();
            var response = responseTask.Result;
            // Assert
            response.EnsureSuccessStatusCode();
        }


        [Fact]
        public void GetConfig()
        {

            _output.WriteLine("log all AddEnvironmentVariables");
            var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();
            foreach (var item in config.AsEnumerable())
            {
                _output.WriteLine(item.Key + " : " + item.Value);
            }


        }

    }
}