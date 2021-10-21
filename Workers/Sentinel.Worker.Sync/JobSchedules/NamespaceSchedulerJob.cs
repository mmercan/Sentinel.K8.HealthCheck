using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using k8s;
using Microsoft.Extensions.Logging;
using Quartz;
using Sentinel.K8s;
using Sentinel.K8s.Watchers;
using Sentinel.Models.K8sDTOs;
using StackExchange.Redis;
using System.Linq;
using Sentinel.Redis;

namespace Sentinel.Worker.Sync.JobSchedules
{
    public class NamespaceSchedulerJob : IJob
    {
        private readonly IKubernetesClient _k8sclient;
        private readonly ILogger<NamespaceSchedulerJob> _logger;
        private readonly IMapper _mapper;
        private readonly RedisDictionary<NamespaceV1> redisDic;

        public NamespaceSchedulerJob(ILogger<NamespaceSchedulerJob> logger, IKubernetesClient k8sclient, IMapper mapper, IConnectionMultiplexer redisMultiplexer)
        {
            _k8sclient = k8sclient;
            _logger = logger;
            _mapper = mapper;

            redisDic = new RedisDictionary<NamespaceV1>(redisMultiplexer, _logger, "Namespaces");
        }
        public async Task Execute(IJobExecutionContext context)
        {
            var items = await _k8sclient.ApiClient.ListNamespaceAsync();
            var dtoitems = _mapper.Map<IList<NamespaceV1>>(items.Items);

            var syncTime = DateTime.UtcNow;
            dtoitems.ForEach(p => p.LatestSyncDateUTC = syncTime);


            redisDic.Sync(dtoitems);

            _logger.LogInformation(dtoitems.Count.ToString() + " Namespaces have been synced");
        }
    }
}