using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Quartz;
using Sentinel.K8s;
using Sentinel.Models.CRDs;
using StackExchange.Redis;
using System.Linq;
using System;
using AutoMapper;
using System.Collections.Generic;
using Sentinel.Models.K8sDTOs;
using Sentinel.Redis;

namespace Sentinel.Worker.Sync.JobSchedules
{
    public class DeploymentSchedulerJob : IJob
    {
        private readonly ILogger<DeploymentSchedulerJob> _logger;
        private readonly IKubernetesClient _k8sclient;
        private readonly IMapper _mapper;
        private readonly IDatabase _redisDatabase;
        private readonly IConnectionMultiplexer redisMultiplexer;

        public DeploymentSchedulerJob(ILogger<DeploymentSchedulerJob> logger, IKubernetesClient k8sclient, IMapper mapper, IConnectionMultiplexer redisMultiplexer)
        {
            _logger = logger;
            _k8sclient = k8sclient;
            _mapper = mapper;
            _redisDatabase = redisMultiplexer.GetDatabase();
            this.redisMultiplexer = redisMultiplexer;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var items = await _k8sclient.ApiClient.ListDeploymentForAllNamespacesAsync();
            var dtoitems = _mapper.Map<IList<DeploymentV1>>(items.Items);
            var syncTime = DateTime.UtcNow;

            foreach (var item in dtoitems)
            {
                //  item.Name = item.Metadata.Name;
                //  item.Namespace = item.Metadata.Namespace;
                item.SyncDate = syncTime;

                // _logger.LogInformation(item.NameandNamespace + " upsert");
            }

            var redisDic = new RedisDictionary<string, DeploymentV1>(redisMultiplexer, _logger, "Deployment");
            redisDic.Sync(dtoitems);
        }
    }
}