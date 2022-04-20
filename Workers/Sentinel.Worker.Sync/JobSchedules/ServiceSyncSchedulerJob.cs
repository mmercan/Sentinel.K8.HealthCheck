using Quartz;
using Sentinel.Models.K8sDTOs;
using Sentinel.Redis;
using StackExchange.Redis;
using Sentinel.K8s.K8sClients;

namespace Sentinel.Worker.Sync.JobSchedules
{
    public class ServiceSyncSchedulerJob : IJob
    {

        private readonly ILogger<ServiceSyncSchedulerJob> _logger;
        private readonly K8sGeneralService _k8sGeneralService;

        private readonly RedisDictionary<ServiceV1> redisDicServices;

        public ServiceSyncSchedulerJob(ILogger<ServiceSyncSchedulerJob> logger, K8sGeneralService k8sGeneralService, IConnectionMultiplexer redisMultiplexer)
        {
            _logger = logger;
            _k8sGeneralService = k8sGeneralService;
            redisDicServices = new RedisDictionary<ServiceV1>(redisMultiplexer, _logger, "Services");
        }

        public Task Execute(IJobExecutionContext context)
        {
            var services = _k8sGeneralService.ServiceClient.GetAllServicesWithDetails();
            redisDicServices.UpSert(services);
            _logger.LogInformation("{ServicesCount} Services have been synced ", services.Count.ToString());
            return Task.CompletedTask;
        }
    }
}