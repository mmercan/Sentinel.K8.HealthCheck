using System.Net.Http.Headers;
using System.Threading;
using Microsoft.AspNetCore.Mvc.Testing;
using Sentinel.Tests.Helpers;
using Sentinel.Worker.Sync.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Worker.Sync.Tests.IntegrationTests
{

    [Collection("WebApplicationFactory")]
    public class StartUpShould
    {

        private WebApplicationFactory<Startup> factory;
        AuthTokenFixture authTokenFixture;
        private ITestOutputHelper output;

        public StartUpShould(CustomWebApplicationFactory factory, AuthTokenFixture authTokenFixture, ITestOutputHelper output)
        {
            this.factory = factory;

            this.output = output;
            this.authTokenFixture = authTokenFixture;
        }


        [Theory]
        [InlineData("/")]
        // [InlineData("/Health/IsAlive")]
        // [InlineData("/Health/IsAliveAndWell")]
        public void Run(string url)
        {

            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(3 * 1000);

            var client = factory.CreateClient();
            // client.DefaultRequestHeaders.Add("api-version", "1.0"); client.DefaultRequestHeaders.Add("Authorization", this.authTokenFixture.Token);
            client.DefaultRequestHeaders.Add("Internal", "true");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            // Act
            var responseTask = client.GetAsync(url);
            responseTask.Wait(source.Token);
            var response = responseTask.Result;
            // Assert
            // response.EnsureSuccessStatusCode();
        }

    }
}