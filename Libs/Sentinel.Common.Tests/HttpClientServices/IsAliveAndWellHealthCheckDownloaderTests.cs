using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Xunit.Abstractions;

namespace Sentinel.Common.Tests.HttpClientServices
{
    public class IsAliveAndWellHealthCheckDownloaderTests
    {
        private ITestOutputHelper output;
        private IConfiguration config;
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
        }



    }
}