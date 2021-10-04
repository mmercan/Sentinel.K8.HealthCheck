using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Sentinel.Common
{

    public static partial class BackgroundServiceHealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddBackgroundServiceHealthCheck(this IHealthChecksBuilder builder, BackgroundServiceWithHealthCheck bgService)
        {
            return builder.AddTypeActivatedCheck<BackgroundServiceHealthCheck>($"BackgroundServiceHealthCheck {bgService.GetType().ToString()}", null, null, bgService);
        }
    }

    public class BackgroundServiceHealthCheck : IHealthCheck
    {
        private DateTime LastProcessUtc;
        private HealthStatus status;
        private int count = 0;
        private string message;

        public void ReportHealthy(string message = null)
        {
            LastProcessUtc = DateTime.UtcNow;
            status = HealthStatus.Healthy;
            this.message = message;
            count++;
        }

        public void ReportUnhealthy(string message = null)
        {
            LastProcessUtc = DateTime.UtcNow;
            status = HealthStatus.Unhealthy;
            this.message = message;
            count++;
        }

        public void ReportDegraded(string message = null)
        {
            LastProcessUtc = DateTime.UtcNow;
            status = HealthStatus.Degraded;
            this.message = message;
            count++;
        }


        public BackgroundServiceHealthCheck()
        {

        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {

            if (LastProcessUtc == null || LastProcessUtc > DateTime.UtcNow.AddHours(-1))
            {

            }

            //var lastProcess = _bgService.Witness.GetLastProcessTime();
            var timeAgo = DateTime.UtcNow.Subtract(LastProcessUtc);
            var data = new Dictionary<string, object> {
                { "Last process", LastProcessUtc },
                { "Time ago", timeAgo },
                {"Count", count.ToString()}
            } as IReadOnlyDictionary<string, object>;


            var result = new HealthCheckResult(status, message, data: data);
            return Task.FromResult(result);

            //Task.FromResult(HealthCheckResult("Processing as much as we can", data));
            // if (lastProcess > DateTime.UtcNow.AddSeconds(-90))
            // {
            //     return Task.FromResult(HealthCheckResult.Healthy("Processing as much as we can", data));
            // }
            // return Task.FromResult(HealthCheckResult.Unhealthy("Processing is stuck somewhere", null, data));
        }
    }
}