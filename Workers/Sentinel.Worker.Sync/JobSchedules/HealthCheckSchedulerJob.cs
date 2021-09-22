using System.Linq;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Quartz;
using Sentinel.K8s;
using Sentinel.Models.CRDs;
using StackExchange.Redis;

namespace Sentinel.Worker.Sync.JobSchedules
{
    public class HealthCheckSchedulerJob : IJob
    {
        private readonly ILogger<HealthCheckSchedulerJob> _logger;
        private readonly IKubernetesClient _k8sclient;
        private readonly IDatabase _database;

        public HealthCheckSchedulerJob(ILogger<HealthCheckSchedulerJob> logger, IKubernetesClient k8sclient, IConnectionMultiplexer redisMultiplexer)
        {
            _logger = logger;
            _k8sclient = k8sclient;
            _database = redisMultiplexer.GetDatabase();
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var checks = await _k8sclient.List<HealthCheckResourceList>();
            var names = string.Join(',', checks.Select(p => p.Name()).ToArray());
            _logger.LogWarning(names);
        }
    }
}