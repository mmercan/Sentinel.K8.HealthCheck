using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sentinel.Common;

namespace Sentinel.Worker.HealthChecker.Subscribers
{
    public class HealthCheckSubscriber : BackgroundServiceWithHealthCheck
    {
        private readonly EasyNetQ.IBus _bus;
        private readonly IConfiguration _configuration;
        private readonly string timezone;
        public HealthCheckSubscriber(
            ILogger<HealthCheckSubscriber> logger,
            IBus bus,
            IOptions<HealthCheckServiceOptions> hcoptions,
            IConfiguration configuration
            ) : base(logger, hcoptions)
        {
            _bus = bus;
            _configuration = configuration;

            if (!string.IsNullOrWhiteSpace(_configuration["timezone"]))
            {
                timezone = _configuration["timezone"];
            }
            else
            {
                timezone = "Australia/Melbourne";
            }
        }


        protected async override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ExecuteOnceAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        private Task ExecuteOnceAsync(CancellationToken stoppingToken)
        {
            // await _bus.SubscribeAsync<HealthCheckRequest>("HealthChecker", async (request, message) =>
            // {
            //     var healthCheckService = new HealthCheckService(_configuration);
            //     var healthCheckResult = await healthCheckService.CheckHealthAsync(stoppingToken);

            //     var response = new HealthCheckResponse
            //     {
            //         HealthCheckResult = healthCheckResult,
            //         Timezone = timezone
            //     };

            //     await _bus.PublishAsync(response);
            // });

            return Task.CompletedTask;
        }
    }
}