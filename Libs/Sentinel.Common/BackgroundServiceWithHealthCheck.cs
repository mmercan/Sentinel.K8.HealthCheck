using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sentinel.Common
{
    public abstract class BackgroundServiceWithHealthCheck : BackgroundService
    {
        protected DateTime lastrestart = DateTime.UtcNow;
        protected readonly ILogger<BackgroundServiceWithHealthCheck> _logger;
        private readonly BackgroundServiceHealthCheck bgHealthCheck = default!;
        protected BackgroundServiceWithHealthCheck(ILogger<BackgroundServiceWithHealthCheck> logger, IOptions<HealthCheckServiceOptions> hcoptions)
        {
            _logger = logger;
            if (hcoptions.Value != null)
            {
                var name = this.GetType().ToString();
                bgHealthCheck = new BackgroundServiceHealthCheck();
                var registration = new HealthCheckRegistration(name, bgHealthCheck, null, null);
                hcoptions.Value.Registrations.Add(registration);
                ReportHealthy(name + " initialized");
            }

        }

        public void ReportHealthy(string message = "") => bgHealthCheck.ReportHealthy(message);
        public void ReportUnhealthy(string message = "") => bgHealthCheck.ReportUnhealthy(message);
        public void ReportDegraded(string message = "") => bgHealthCheck.ReportDegraded(message);

    }

}