// using System.Net.Http.Headers;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc.Testing;
// using Sentinel.Tests.Helpers;
// using Sentinel.Worker.Sync.Tests.Helpers;
// using Xunit;
// using Xunit.Abstractions;

// namespace Sentinel.Worker.Sync.Tests.IntegrationTests
// {

//     [Collection("WebApplicationFactory")]
//     public class StartUpShould
//     {

//         private WebApplicationFactory<Program> factory;
//         AuthTokenFixture authTokenFixture;
//         private ITestOutputHelper _output;

//         public StartUpShould(CustomWebApplicationFactory factory, AuthTokenFixture authTokenFixture, ITestOutputHelper output)
//         {
//             this.factory = factory;

//             _output = output;
//             this.authTokenFixture = authTokenFixture;
//         }


//         [Theory]
//         [InlineData("/")]
//         // [InlineData("/Health/IsAlive")]
//         // [InlineData("/Health/IsAliveAndWell")]
//         public void Run(string url)
//         {
//             Task.Run(() =>
//             {
//                 _output.WriteLine("Run is started");
//                 factory.ConfigureAwait(false);

//                 var client = factory.CreateClient();

//                 // client.DefaultRequestHeaders.Add("api-version", "1.0"); client.DefaultRequestHeaders.Add("Authorization", this.authTokenFixture.Token);
//                 client.DefaultRequestHeaders.Add("Internal", "true");
//                 client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//                 // Act
//                 var responseTask = client.GetAsync(url);
//                 responseTask.Wait(10000);
//                 var response = responseTask.Result;
//                 _output.WriteLine("Response Received");
//                 client.Dispose();
//                 // Assert
//                 // response.EnsureSuccessStatusCode();
//             }).Wait(40000);

//             factory.Dispose();
//         }

//     }
// }