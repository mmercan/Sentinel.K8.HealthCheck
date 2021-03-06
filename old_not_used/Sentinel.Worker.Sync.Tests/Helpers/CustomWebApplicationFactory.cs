using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;


namespace Sentinel.Worker.Sync.Tests.Helpers
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder
                .ConfigureAppConfiguration(config => config
                    .AddJsonFile("appsettings.docker.tests.json", false)
                )
                .UseKestrel(o => { o.Limits.KeepAliveTimeout = TimeSpan.FromSeconds(10); });
        }
    }
}