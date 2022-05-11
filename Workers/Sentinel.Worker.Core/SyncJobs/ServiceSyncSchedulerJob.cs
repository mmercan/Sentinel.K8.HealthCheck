using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Libs.Sentinel.K8s;
using Quartz;
using Sentinel.K8s.K8sClients;
using Sentinel.Models.K8sDTOs;
using Sentinel.Models.Redis;
using Sentinel.Redis;
using StackExchange.Redis;

namespace Workers.Sentinel.Worker.Core.SyncJobs
{
    [QuartzJob(ConfigurationSection = "Schedules:ServicesSynccheduler", DelaySecond = 1)]
    public class ServiceSyncSchedulerJob : IJob
    {
        private readonly ILogger<ServiceSyncSchedulerJob> _logger;
        private readonly K8sGeneralService _k8sGeneralService;
        private readonly K8MemoryRepository _k8MemoryRepository;
        private readonly IRedisDictionary<ServiceV1> redisDicServices;
        public ServiceSyncSchedulerJob(ILogger<ServiceSyncSchedulerJob> logger, K8sGeneralService k8sGeneralService,
        K8MemoryRepository k8MemoryRepository, IConnectionMultiplexer redisMultiplexer)
        {
            _logger = logger;
            _k8sGeneralService = k8sGeneralService;
            _k8MemoryRepository = k8MemoryRepository;
            redisDicServices = new RedisDictionary<ServiceV1>(redisMultiplexer, _logger, "Services");
        }

        public Task Execute(IJobExecutionContext context)
        {
            //get values
            var services = _k8sGeneralService.ServiceClient.GetAllServicesWithDetails();
            //sync values
            _k8MemoryRepository.Services = services;
            redisDicServices.Sync(services, true);
            _logger.LogInformation("{ServicesCount} Services have been synced ", services.Count.ToString());
            return Task.CompletedTask;
        }
    }
}