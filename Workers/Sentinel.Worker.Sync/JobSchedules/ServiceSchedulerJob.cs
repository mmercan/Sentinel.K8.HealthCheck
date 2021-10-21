using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Quartz;
using Sentinel.K8s;
using Sentinel.Models.K8sDTOs;
using Sentinel.Redis;
using StackExchange.Redis;
using System.Linq;

namespace Sentinel.Worker.Sync.JobSchedules
{
    public class ServiceSchedulerJob : IJob
    {
        private readonly IKubernetesClient _k8sclient;
        private readonly ILogger<ServiceSchedulerJob> _logger;
        private readonly IMapper _mapper;
        private readonly RedisDictionary<ServiceV1> redisDic;

        public ServiceSchedulerJob(ILogger<ServiceSchedulerJob> logger, IKubernetesClient k8sclient, IMapper mapper, IConnectionMultiplexer redisMultiplexer)
        {
            _k8sclient = k8sclient;
            _logger = logger;
            _mapper = mapper;
            redisDic = new RedisDictionary<ServiceV1>(redisMultiplexer, _logger, "Services");
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var services = await _k8sclient.ApiClient.ListServiceForAllNamespacesWithHttpMessagesAsync();
            var dtoitems = _mapper.Map<IList<ServiceV1>>(services.Body.Items);

            var syncTime = DateTime.UtcNow;
            dtoitems.ForEach(p => p.LatestSyncDateUTC = syncTime);

            redisDic.Sync(dtoitems);
            _logger.LogInformation(dtoitems.Count.ToString() + " Services have been synced");
        }
    }
}