using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Quartz;
using Sentinel.K8s.K8sClients;
using Sentinel.Models.CRDs;
using Sentinel.Models.K8sDTOs;
using Sentinel.Models.Redis;
using Sentinel.Redis;
using StackExchange.Redis;

namespace Workers.Sentinel.Worker.Core.SyncJobs
{
    [QuartzJob(ConfigurationSection = "Schedules:HealthCheckSyncScheduler")]
    public class HealthCheckSyncSchedulerJob : IJob
    {
        private readonly ILogger<HealthCheckSyncSchedulerJob> _logger;
        private readonly K8sGeneralService _k8sGeneralService;
        private readonly IMapper _mapper;
        private readonly IRedisDictionary<HealthCheckResourceV1> redisDic;

        public HealthCheckSyncSchedulerJob(ILogger<HealthCheckSyncSchedulerJob> logger, K8sGeneralService k8sGeneralService, IMapper mapper, IConnectionMultiplexer redisMultiplexer)
        {
            _logger = logger;
            _k8sGeneralService = k8sGeneralService;
            _mapper = mapper;
            redisDic = new RedisDictionary<HealthCheckResourceV1>(redisMultiplexer, _logger, "HealthChecks");
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var checks = await _k8sGeneralService.HealthCheckResourceClient.GetAllHealthCheckResourcesAsync();
            checks.ForEach(async check =>
            {
                if (string.IsNullOrEmpty(check.Status?.Phase))
                {
                    await _k8sGeneralService.HealthCheckResourceClient.UpdateStartusAsync(check, HealthCheckResource.HealthCheckResourceStatusPhase.AddedtoRedis);
                }
            });
            var dtoitems = _mapper.Map<IList<HealthCheckResourceV1>>(checks);

            var syncTime = DateTime.UtcNow;
            dtoitems.ForEach(p => p.LatestSyncDateUTC = syncTime);

            redisDic.Sync(dtoitems);
            _logger.LogInformation(checks.Count.ToString() + " HealthChecks have been synced");
        }
    }
}