using AutoMapper;
using k8s;
using Quartz;
using Sentinel.K8s;
using Sentinel.Models.K8sDTOs;
using StackExchange.Redis;
using Sentinel.Redis;
using Sentinel.Models.Redis;

namespace Sentinel.Worker.Sync.JobSchedules
{
    public class NamespaceSyncSchedulerJob : IJob
    {
        private readonly IKubernetesClient _k8sclient;
        private readonly ILogger<NamespaceSyncSchedulerJob> _logger;
        private readonly IMapper _mapper;
        private readonly IRedisDictionary<NamespaceV1> redisDic;

        public NamespaceSyncSchedulerJob(ILogger<NamespaceSyncSchedulerJob> logger, IKubernetesClient k8sclient, IMapper mapper, IConnectionMultiplexer redisMultiplexer)
        {
            _k8sclient = k8sclient;
            _logger = logger;
            _mapper = mapper;
            redisDic = new RedisDictionary<NamespaceV1>(redisMultiplexer, _logger, "Namespaces");
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var items = await _k8sclient.ListNamespaceAsync();
            var dtoitems = _mapper.Map<IList<NamespaceV1>>(items);

            var syncTime = DateTime.UtcNow;
            dtoitems.ForEach(p => p.LatestSyncDateUTC = syncTime);
            redisDic.Sync(dtoitems);

            _logger.LogInformation("{NamespaceCount} Namespaces have been synced", dtoitems.Count.ToString());
        }
    }
}