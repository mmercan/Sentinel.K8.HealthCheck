using k8s;
using Quartz;
using Sentinel.K8s;
using StackExchange.Redis;
using AutoMapper;
using Sentinel.Models.K8sDTOs;
using Sentinel.Redis;

namespace Sentinel.Worker.Sync.JobSchedules
{
    public class DeploymentSyncSchedulerJob : IJob
    {
        private readonly ILogger<DeploymentSyncSchedulerJob> _logger;
        private readonly IKubernetesClient _k8sclient;
        private readonly IMapper _mapper;
        private readonly RedisDictionary<DeploymentV1> redisDic;

        public DeploymentSyncSchedulerJob(ILogger<DeploymentSyncSchedulerJob> logger, IKubernetesClient k8sclient, IMapper mapper, IConnectionMultiplexer redisMultiplexer)
        {
            _logger = logger;
            _k8sclient = k8sclient;
            _mapper = mapper;
            redisDic = new RedisDictionary<DeploymentV1>(redisMultiplexer, _logger, "Deployment");

        }

        public async Task Execute(IJobExecutionContext context)
        {
            var items = await _k8sclient.ApiClient.ListDeploymentForAllNamespacesAsync();
            var dtoitems = _mapper.Map<IList<DeploymentV1>>(items.Items);

            var syncTime = DateTime.UtcNow;
            dtoitems.ForEach(p => p.SyncDate = syncTime);
            redisDic.UpSert(dtoitems);
            _logger.LogInformation(dtoitems.Count.ToString() + " Deployments have been synced");
        }
    }
}