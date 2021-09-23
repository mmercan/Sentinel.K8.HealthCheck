using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Sentinel.K8s;

namespace Sentinel.Worker.Sync.JobSchedules
{
    public class ServiceSchedulerJob : IJob
    {

        private readonly IKubernetesClient _client;
        private readonly ILogger<ServiceSchedulerJob> _logger;

        public ServiceSchedulerJob(IKubernetesClient client, ILogger<ServiceSchedulerJob> logger)
        {
            _client = client;
            _logger = logger;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var services = await _client.ApiClient.ListServiceForAllNamespacesWithHttpMessagesAsync();

            await Task.Delay(TimeSpan.FromSeconds(15));
            _logger.LogCritical("current NS : " + _client.ToString());
        }
    }
}