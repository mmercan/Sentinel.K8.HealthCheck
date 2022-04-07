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
using Newtonsoft.Json.Linq;
using Sentinel.K8s.K8sClients;

namespace Sentinel.Worker.Sync.JobSchedules
{
    public class ServiceSchedulerJob : IJob
    {

        private readonly ILogger<ServiceSchedulerJob> _logger;
        private readonly K8sGeneralService _k8sGeneralService;

        private readonly RedisDictionary<ServiceV1> redisDicServices;

        public ServiceSchedulerJob(ILogger<ServiceSchedulerJob> logger, K8sGeneralService k8sGeneralService, IConnectionMultiplexer redisMultiplexer)
        {
            _logger = logger;
            _k8sGeneralService = k8sGeneralService;
            redisDicServices = new RedisDictionary<ServiceV1>(redisMultiplexer, _logger, "Services");
        }

        public Task Execute(IJobExecutionContext context)
        {
            var services = _k8sGeneralService.ServiceClient.GetAllServicesWithDetails();
            redisDicServices.UpSert(services);
            //  _logger.LogInformation(services.Count.ToString() + " Services have been synced (" + ingresses.Count.ToString() + " ingresses) (" + virtualservices.Count.ToString() + " virtualservices) merged ");
            _logger.LogInformation(services.Count.ToString() + " Services have been synced "); //(" + ingresses.Count.ToString() + " ingresses) (" + virtualservices.Count.ToString() + " virtualservices) merged ");
            return Task.CompletedTask;
        }
    }
}