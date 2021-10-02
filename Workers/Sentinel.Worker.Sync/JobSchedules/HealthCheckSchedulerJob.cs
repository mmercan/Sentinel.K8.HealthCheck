using System;
using System.Linq;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Quartz;
using Sentinel.K8s;
using Sentinel.Models.CRDs;
using Sentinel.Redis;
using StackExchange.Redis;

namespace Sentinel.Worker.Sync.JobSchedules
{
    public class HealthCheckSchedulerJob : IJob
    {
        private readonly ILogger<HealthCheckSchedulerJob> _logger;
        private readonly IKubernetesClient _k8sclient;
        private readonly IConnectionMultiplexer _redisMultiplexer;

        public HealthCheckSchedulerJob(ILogger<HealthCheckSchedulerJob> logger, IKubernetesClient k8sclient, IConnectionMultiplexer redisMultiplexer)
        {
            _logger = logger;
            _k8sclient = k8sclient;
            _redisMultiplexer = redisMultiplexer;
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var checks = await _k8sclient.List<HealthCheckResource>();

            var syncTime = DateTime.UtcNow;

            foreach (var item in checks)
            {
                item.SyncDate = syncTime;
            }


            var str = checks.ToJSON();

            var redisDic = new RedisDictionary<string, HealthCheckResource>(_redisMultiplexer, _logger, "HealthChecks");
            redisDic.Sync(checks);
        }
    }
}