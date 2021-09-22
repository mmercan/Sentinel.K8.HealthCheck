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
            return builder.AddTypeActivatedCheck<BackgroundServiceHealthCheck>($"RedisHealthCheck {bgService.GetType().ToString()}", null, null, bgService);
        }
    }

    public class BackgroundServiceHealthCheck : IHealthCheck
    {

        private readonly BackgroundServiceWithHealthCheck _bgService;
        public BackgroundServiceHealthCheck(BackgroundServiceWithHealthCheck bgService)
        {
            _bgService = bgService;
        }

        public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            var lastProcess = _bgService.Witness.GetLastProcessTime();
            var timeAgo = DateTime.UtcNow.Subtract(lastProcess);
            var data = new Dictionary<string, object> {
                { "Last process", lastProcess },
                { "Time ago", timeAgo },
                {"Count", _bgService.Witness.GetCount().ToString()}
            } as IReadOnlyDictionary<string, object>;

            if (lastProcess > DateTime.UtcNow.AddSeconds(-90))
            {
                return Task.FromResult(HealthCheckResult.Healthy("Processing as much as we can", data));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy("Processing is stuck somewhere", null, data));
        }
    }
}