using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using k8s;
using Libs.Sentinel.K8s;
using Quartz;
using Sentinel.K8s;
using Sentinel.Models.K8sDTOs;
using Sentinel.Models.Redis;
using Sentinel.Redis;
using StackExchange.Redis;

namespace Workers.Sentinel.Worker.Core.SyncJobs
{
    [QuartzJob(ConfigurationSection = "Schedules:DeploymentSyncScheduler")]
    public class DeploymentSyncSchedulerJob : IJob
    {
        private readonly ILogger<DeploymentSyncSchedulerJob> _logger;
        private readonly IKubernetesClient _k8sclient;
        private readonly IMapper _mapper;
        private readonly K8MemoryRepository _k8MemoryRepository;
        private readonly IRedisDictionary<DeploymentV1> redisDic;

        public DeploymentSyncSchedulerJob(ILogger<DeploymentSyncSchedulerJob> logger,
        K8MemoryRepository k8MemoryRepository, IKubernetesClient k8sclient, IMapper mapper, IConnectionMultiplexer redisMultiplexer)
        {
            _logger = logger;
            _k8sclient = k8sclient;
            _mapper = mapper;
            _k8MemoryRepository = k8MemoryRepository;
            redisDic = new RedisDictionary<DeploymentV1>(redisMultiplexer, _logger, "Deployment");
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var items = await _k8sclient.ApiClient.ListDeploymentForAllNamespacesAsync();
            var dtoitems = _mapper.Map<IList<DeploymentV1>>(items.Items);

            var syncTime = DateTime.UtcNow;
            dtoitems.ForEach(p => p.SyncDate = syncTime);
            _k8MemoryRepository.Deployments = dtoitems;
            redisDic.Sync(dtoitems);
            _logger.LogInformation(dtoitems.Count.ToString() + " Deployments have been synced");
        }
    }
}