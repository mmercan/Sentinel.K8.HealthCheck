using System;
using System.Threading.Tasks;
using k8s;
using Microsoft.Extensions.Logging;
using Quartz;
using Sentinel.K8s;
using Sentinel.K8s.Watchers;

namespace Sentinel.Worker.Sync.JobSchedules
{
    public class NamespaceSchedulerJob : IJob
    {
        private readonly IKubernetesClient _client;
        private readonly ILogger<NamespaceSchedulerJob> _logger;

        public NamespaceSchedulerJob(IKubernetesClient client, ILogger<NamespaceSchedulerJob> logger)
        {
            _client = client;
            _logger = logger;
        }
        public async Task Execute(IJobExecutionContext context)
        {


            var ns = await _client.GetCurrentNamespace();

            await Task.Delay(TimeSpan.FromSeconds(15));
            _logger.LogCritical("current NS : " + ns);

            // var executingTask = Task.Factory.StartNew(async () =>
            //  {
            //      //Pull Namespaces
            //      // Mapped to the to be stored
            //      // Store it on desired DB
            //  }, TaskCreationOptions.LongRunning);


            // if (executingTask.IsCompleted)
            // {
            //     return executingTask;
            // }
            // return Task.CompletedTask;
        }
    }
}