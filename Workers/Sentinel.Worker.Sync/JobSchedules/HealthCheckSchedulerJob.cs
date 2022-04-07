using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Quartz;
using Sentinel.K8s;
using Sentinel.K8s.K8sClients;
using Sentinel.Models.CRDs;
using Sentinel.Models.K8sDTOs;
using Sentinel.Redis;
using StackExchange.Redis;

namespace Sentinel.Worker.Sync.JobSchedules
{
    public class HealthCheckSchedulerJob : IJob
    {
        private readonly ILogger<HealthCheckSchedulerJob> _logger;
        private readonly K8sGeneralService _k8sGeneralService;
        private readonly IMapper _mapper;
        private readonly RedisDictionary<HealthCheckResourceV1> redisDic;

        public HealthCheckSchedulerJob(ILogger<HealthCheckSchedulerJob> logger, K8sGeneralService k8sGeneralService, IMapper mapper, IConnectionMultiplexer redisMultiplexer)
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

            redisDic.UpSert(dtoitems);
            _logger.LogInformation(checks.Count.ToString() + " HealthChecks have been synced");
        }
    }
}