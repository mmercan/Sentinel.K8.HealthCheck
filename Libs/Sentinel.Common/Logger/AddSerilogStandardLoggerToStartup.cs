using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Polly;
using Polly.Extensions.Http;
using System.Linq;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Sentinel.Common
{
    public static class LoggerHelper
    {

        public static LoggerConfiguration ConfigureLogger(
            string applicationName,
            string environmentName,
            IConfiguration configuration,
            LogEventLevel minimumLogLevel = LogEventLevel.Information)
        {
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Enviroment", environmentName)
                .Enrich.WithProperty("ApplicationName", applicationName)
                .MinimumLevel.Override("Microsoft", minimumLogLevel)
                .WriteTo.Console()
                .WriteTo.File("Logs/logs.txt", rollingInterval: RollingInterval.Day);
            logger.WriteTo.Console();
            logger.WriteTo.File("Logs/logs.txt", rollingInterval: RollingInterval.Day);

            return logger;
        }

        public static void UseSerilogAuto(
            this IHostBuilder builder, string applicationName,
            string environmentName, LogEventLevel minimumLogLevel = LogEventLevel.Information, LogEventLevel AspNetCoreminimumLogLevel = LogEventLevel.Warning)
        {

            builder.UseSerilog((ctx, lc) => lc
                .Enrich.FromLogContext()
                .Enrich.WithProperty("Enviroment", environmentName)
                .Enrich.WithProperty("ApplicationName", applicationName)
                .MinimumLevel.Override("Microsoft.AspNetCore", AspNetCoreminimumLogLevel)
                .MinimumLevel.Override("Default", minimumLogLevel)
                .WriteTo.Console()
                .WriteTo.File("Logs/logs.txt", rollingInterval: RollingInterval.Day)
            );



        }

    }

}