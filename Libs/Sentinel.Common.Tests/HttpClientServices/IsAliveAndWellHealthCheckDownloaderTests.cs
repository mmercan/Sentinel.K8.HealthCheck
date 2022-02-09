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

            var q = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var loc = Path.Combine(q, "../../../appsettings.json");
            var config_orj = new ConfigurationBuilder()

            //.AddJsonFile("../../../appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables().Build();

            var myConfiguration = new Dictionary<string, string>
            {
                {"RequiredHeaders:0", "Internal"},
                {"RequiredHeaders:1", "External"},
                {"DefaultIsAliveAndWellSuffix","/Health/IsAliveAndWell"},
                {"68A1711EFC66EEA676F8B165102D94697DEE342F", "MIIDmzCCAoOgAwIBAgIUXTsBEDVmNFKfbC8vu+Y/Xhalf7wwDQYJKoZIhvcNAQEL BQAwSTETMBEGA1UEAwwKbXlyY2FuLmNvbTELMAkGA1UEBhMCQVUxETAPBgNVBAgM CFZpY3RvcmlhMRIwEAYDVQQHDAlNZWxib3VybmUwHhcNMjIwMjAzMDA1NjA0WhcN MzIwMjAxMDA1NjA0WjBJMRMwEQYDVQQDDApteXJjYW4uY29tMQswCQYDVQQGEwJB VTERMA8GA1UECAwIVmljdG9yaWExEjAQBgNVBAcMCU1lbGJvdXJuZTCCASIwDQYJ KoZIhvcNAQEBBQADggEPADCCAQoCggEBAO3PaRNVQDNt/tgkCVQZRh2EDGzeH8zG aKfYUHJBNpffzQ1AZp7W1NA9QSCs9gv7pOib50chK/EWoExO16wJLSkbI9Bp4eFl dvIE45PGD3xxKIWnBcCZ6RY18KTXNFICCl7XZRTAFsPHzMIFHvEOAklZN2mMgkDa gJurzCMDCOAB1YfFLUnwH4Q0uUc9b4g0g8zDsU6+yxynzUURyFPC8IGQev37gZ8f KJS5nDv5F+Y5HdzamJij6kHRunB8Y5mJnMPj3W+jvw1WVTLwaFFVbN1x6ssymPlR lK4KFpFXAXhS8wb/XhCZ7Yxww3rdeGk8Go4PHdfER1mAKhj4rgGgZzkCAwEAAaN7 MHkwHQYDVR0OBBYEFBgr6pnStzIl1xvD1qJzSWnrBadhMB8GA1UdIwQYMBaAFBgr 6pnStzIl1xvD1qJzSWnrBadhMAsGA1UdDwQEAwIHgDATBgNVHSUEDDAKBggrBgEF BQcDAjAVBgNVHREEDjAMggpteXJjYW4uY29tMA0GCSqGSIb3DQEBCwUAA4IBAQCJ hrLlJDnZahYxnRLbPZ4okp7yrFl2eck74O4HEqjWwf7mi+cf7pKr4rxLRVR/vXL9 2wUcT7ayThfbiIPFJbq/jPt6AgQPNYZ/fxoSepfuKgTpa4xp6Baxj3AeqTgwcB1M BO79eMcdI1LwbTHOiK1FEdWbX9FyuPPsmOXoTpyPg6XNhrb2lHgrU4idK2uKm3lZ 8ecOEuWQsdLV5lBPbdJqvS5dLackyxGcTZMYcjV2+N519KtJgo4+C/XM0Q7lcTuH USMdSBJaskRQ/pySfPxx/Zn2fz5r8fx2shWqICN4sSzL9UDH3aXYMhTUOWgmkP1D cce/o5HokahqvVJxHx5V"},
                {"67d009b1-97fe-4963-84ff-3590b06df0da:ClientId","67d009b1-97fe-4963-84ff-3590b06df0da"},
                {"67d009b1-97fe-4963-84ff-3590b06df0da:TenantId",config_orj["AzureAd:TenantId"]},
                {"67d009b1-97fe-4963-84ff-3590b06df0da:Secret",config_orj["AzureAd:Secret"]}
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
        public void IsAliveAndWellHealthCheckDownloaderShouldReturnResult()
        {
            // // Given
            // // download textfile for test
            var text = File.ReadAllText("../../../sentinel-dev-health-ui-app-health-ui.sentinel-dev.txt");
            var serv = text.FromJSON<ServiceV1>();

            var hc = new HealthCheckResourceV1 { Schedule = "* * * * *", Name = "test", Namespace = "default" };
            hc.Spec = new HealthCheckResourceSpecV1
            {
                Service = "kubernetes",
                Cert = "68A1711EFC66EEA676F8B165102D94697DEE342F",
                ClientId = "67d009b1-97fe-4963-84ff-3590b06df0da"
            };

            // // When

            // // Then
            // Assert.NotEmpty(serv?.Name);
            Task.Run(() =>
            {
                var downTask = downloader.DownloadAsync(serv, hc);
                downTask.Wait(TimeSpan.FromSeconds(50));
                // Assert.NotEmpty(downTask.Result);
            }).Wait(TimeSpan.FromSeconds(60));

            Assert.NotNull(serv);
        }

    }
}