using AutoMapper;
using Quartz;
using Sentinel.K8s;
using Sentinel.Models.CRDs;
using Sentinel.Models.K8sDTOs;
using Sentinel.Redis;
using StackExchange.Redis;

namespace Sentinel.Worker.Sync.JobSchedules
{
    public class DeploymentScalersSyncShedulerJob : IJob
    {
        private readonly ILogger<DeploymentScalersSyncShedulerJob> _logger;
        private readonly IKubernetesClient _k8sclient;
        private readonly IMapper _mapper;
        private readonly RedisDictionary<DeploymentScalerResourceV1> redisDic;

        public DeploymentScalersSyncShedulerJob(ILogger<DeploymentScalersSyncShedulerJob> logger, IKubernetesClient k8sclient, IMapper mapper, IConnectionMultiplexer redisMultiplexer)
        {
            _logger = logger;
            _k8sclient = k8sclient;
            _mapper = mapper;
            redisDic = new RedisDictionary<DeploymentScalerResourceV1>(redisMultiplexer, _logger, "DeploymentScalers");
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var checks = await _k8sclient.ListAsync<DeploymentScalerResource>();
            var dtoitems = _mapper.Map<IList<DeploymentScalerResourceV1>>(checks);

            var syncTime = DateTime.UtcNow;
            dtoitems.ForEach(p => p.LatestSyncDateUTC = syncTime);

            redisDic.Sync(dtoitems);
            _logger.LogInformation("{DeploymentScalerCount} DeploymentScalerResource have been synced", checks.Count.ToString());
        }
    }
}