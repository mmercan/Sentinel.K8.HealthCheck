using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Libs.Sentinel.K8s;
using Quartz;
using Sentinel.K8s;
using Sentinel.Models.CRDs;
using Sentinel.Models.K8sDTOs;
using Sentinel.Redis;
using StackExchange.Redis;

namespace Workers.Sentinel.Worker.Core.SyncJobs
{
    [QuartzJob(ConfigurationSection = "Schedules:DeploymentScalerSyncScheduler")]
    public class DeploymentScalersSyncShedulerJob : IJob
    {
        private readonly ILogger<DeploymentScalersSyncShedulerJob> _logger;
        private readonly IKubernetesClient _k8sclient;
        private readonly IMapper _mapper;
        private readonly K8MemoryRepository _k8MemoryRepository;
        private readonly RedisDictionary<DeploymentScalerResourceV1> redisDic;

        public DeploymentScalersSyncShedulerJob(ILogger<DeploymentScalersSyncShedulerJob> logger,
        IKubernetesClient k8sclient, IMapper mapper, K8MemoryRepository k8MemoryRepository, IConnectionMultiplexer redisMultiplexer)
        {
            _logger = logger;
            _k8sclient = k8sclient;
            _mapper = mapper;
            _k8MemoryRepository = k8MemoryRepository;
            redisDic = new RedisDictionary<DeploymentScalerResourceV1>(redisMultiplexer, _logger, "DeploymentScalers");
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var checks = await _k8sclient.ListAsync<DeploymentScalerResource>();
            var dtoitems = _mapper.Map<IList<DeploymentScalerResourceV1>>(checks);

            var syncTime = DateTime.UtcNow;
            dtoitems.ForEach(p => p.LatestSyncDateUTC = syncTime);
            _k8MemoryRepository.DeploymentScalers = dtoitems;
            redisDic.Sync(dtoitems);
            _logger.LogInformation(checks.Count.ToString() + " DeploymentScalerResource have been synced");
        }
    }
}