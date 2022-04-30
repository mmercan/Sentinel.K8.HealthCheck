using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Libs.Sentinel.K8s;
using Quartz;
using Sentinel.K8s;
using Sentinel.Models.K8sDTOs;
using Sentinel.Models.Redis;
using Sentinel.Redis;
using StackExchange.Redis;

namespace Workers.Sentinel.Worker.Core.SyncJobs
{
    [QuartzJob(ConfigurationSection = "Schedules:NamespaceSyncScheduler")]
    public class NamespaceSyncSchedulerJob : IJob
    {
        private readonly IKubernetesClient _k8sclient;
        private readonly ILogger<NamespaceSyncSchedulerJob> _logger;
        private readonly IMapper _mapper;
        private readonly K8MemoryRepository _k8MemoryRepository;
        private readonly IRedisDictionary<NamespaceV1> redisDic;

        public NamespaceSyncSchedulerJob(ILogger<NamespaceSyncSchedulerJob> logger,
        K8MemoryRepository k8MemoryRepository, IKubernetesClient k8sclient, IMapper mapper, IConnectionMultiplexer redisMultiplexer)
        {
            _k8sclient = k8sclient;
            _logger = logger;
            _mapper = mapper;
            _k8MemoryRepository = k8MemoryRepository;
            redisDic = new RedisDictionary<NamespaceV1>(redisMultiplexer, _logger, "Namespaces");
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var items = await _k8sclient.ListNamespaceAsync();
            var dtoitems = _mapper.Map<IList<NamespaceV1>>(items);

            var syncTime = DateTime.UtcNow;
            dtoitems.ForEach(p => p.LatestSyncDateUTC = syncTime);
            _k8MemoryRepository.Namespaces = dtoitems;
            redisDic.Sync(dtoitems);

            _logger.LogInformation("{NamespaceCount} Namespaces have been synced", dtoitems.Count.ToString());
        }
    }
}