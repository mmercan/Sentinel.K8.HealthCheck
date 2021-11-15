using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using Polly;
using Polly.Extensions.Http;
using System.Linq;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Configuration;

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

            return logger;
            // loggerFactory.AddSerilog();
            // Log.Logger = logger.CreateLogger();

        }

    }

}