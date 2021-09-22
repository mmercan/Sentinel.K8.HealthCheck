using System;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sentinel.Common;
using Sentinel.K8s;

namespace Sentinel.Worker.Sync.Watchers
{
    public class NamespaceWatcherJob : BackgroundServiceWithHealthCheck
    {
        private readonly IKubernetesClient _client;
        public IServiceProvider Services { get; }

        public NamespaceWatcherJob(IServiceProvider serviceProvider, ILogger<NamespaceWatcherJob> logger,
        IServiceCollection services, IKubernetesClient client) : base(serviceProvider, logger, services)
        {
            _client = client;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Consume Scoped Service Hosted Service running.");

            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Consume Scoped Service Hosted Service is working.");


            await _client.Watch<k8s.Models.V1Namespace>(timeout: TimeSpan.FromMinutes(3),
            onEvent: OnEvent,
            onError: (error) =>
             {
                 _logger.LogWarning("Error NamespaceWatcherJob : ", error.Message);
             },
             onClose: () =>
             {

             },
             cancellationToken: stoppingToken);

            // using (var scope = Services.CreateScope())
            // {
            //     // var scopedProcessingService = 
            //     //     scope.ServiceProvider
            //     //         .GetRequiredService<IScopedProcessingService>();

            //     await scopedProcessingService.DoWork(stoppingToken);
            // }
            // await Task.Delay(100);
            //return Task.CompletedTask;
        }

        private void OnEvent(WatchEventType arg1, V1Namespace arg2)
        {
            // throw new NotImplementedException();
        }

        // private void OnEvent(WatchEventType type, V1Deployment item)
        // {
        //     this._logger.LogCritical("==on watch event==");
        //     this._logger.LogCritical(type.ToString());
        //     this._logger.LogCritical(item.Metadata.Name);
        //     this._logger.LogCritical("===on watch event===");
        //     //SavetoCache(item).Wait();
        // }

        private void OnError(Exception ex)
        {
            this._logger.LogError("===on watch Exception : " + ex.Message);
        }

        private void OnClosed()
        {
            // var utc = DateTime.UtcNow.ToString();
            // var howlongran = (DateTime.UtcNow - lastrestart);

            // this._logger.LogError("===on watch Connection  Closed after " + howlongran.TotalMinutes.ToString() + ":" + howlongran.Seconds.ToString() + " min:sec : re-running delay 30 seconds " + utc);


            // Task.Delay(TimeSpan.FromSeconds(30)).Wait();
            // lastrestart = DateTime.UtcNow;
            // this._logger.LogError("=== on watch Restarting Now.... ===" + lastrestart.ToString());
            // executingTask = Task.Factory.StartNew(new Action(deployWatchStarter), TaskCreationOptions.LongRunning);
        }



        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Consume Scoped Service Hosted Service is stopping.");
            await base.StopAsync(cancellationToken);
        }

    }
}