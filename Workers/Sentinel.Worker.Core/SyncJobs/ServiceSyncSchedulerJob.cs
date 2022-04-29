using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quartz;
using Sentinel.K8s.K8sClients;
using Sentinel.Models.K8sDTOs;
using Sentinel.Models.Redis;
using Sentinel.Redis;
using StackExchange.Redis;

namespace Workers.Sentinel.Worker.Core.SyncJobs
{
    [QuartzJob(ConfigurationSection = "Schedules:ServicesSynccheduler")]
    public class ServiceSyncSchedulerJob : IJob
    {
        private readonly ILogger<ServiceSyncSchedulerJob> _logger;
        private readonly K8sGeneralService _k8sGeneralService;
        private readonly IRedisDictionary<ServiceV1> redisDicServices;
        public ServiceSyncSchedulerJob(ILogger<ServiceSyncSchedulerJob> logger, K8sGeneralService k8sGeneralService, IConnectionMultiplexer redisMultiplexer)
        {
            _logger = logger;
            _k8sGeneralService = k8sGeneralService;
            redisDicServices = new RedisDictionary<ServiceV1>(redisMultiplexer, _logger, "Services");
        }

        public Task Execute(IJobExecutionContext context)
        {
            //get values
            var services = _k8sGeneralService.ServiceClient.GetAllServicesWithDetails();
            //sync values
            redisDicServices.Sync(services, true);
            _logger.LogInformation("{ServicesCount} Services have been synced ", services.Count.ToString());
            return Task.CompletedTask;
        }
    }
}