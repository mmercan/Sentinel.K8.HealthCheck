using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using k8s;
using k8s.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sentinel.Common;
using Sentinel.K8s;
using Sentinel.Models.K8sDTOs;
using Sentinel.Redis;
using StackExchange.Redis;

namespace Sentinel.Worker.Sync.Watchers
{
    public class NamespaceWatcherJob : BackgroundServiceWithHealthCheck
    {
        private readonly IKubernetesClient _k8sService;
        private readonly IMapper _mapper;
        private readonly RedisDictionary<NamespaceV1> redisDic;
        private Task executingTask;
        private CancellationToken cancellationToken;

        public NamespaceWatcherJob(
            ILogger<NamespaceWatcherJob> logger,
            IKubernetesClient k8sService,
            IConnectionMultiplexer redisMultiplexer,
            IMapper mapper,
            IOptions<HealthCheckServiceOptions> hcoptions) : base(logger, hcoptions)
        {
            _k8sService = k8sService;
            _mapper = mapper;
            redisDic = new RedisDictionary<NamespaceV1>(redisMultiplexer, _logger, "NamespacesWatch");
        }
        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            cancellationToken = stoppingToken;
            //executingTask = Task.Factory.StartNew(new Action(namespaceWatchStarter), TaskCreationOptions.LongRunning);
            executingTask = Task.Factory.StartNew(() => namespaceWatchStarter(stoppingToken), TaskCreationOptions.LongRunning);
            if (executingTask.IsCompleted)
            {
                return executingTask;
            }
            return Task.CompletedTask;
        }

        private void namespaceWatchStarter(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Consume Scoped Service Hosted Service is working.");
            this.ReportHealthy();

            using (_k8sService.Watch<k8s.Models.V1Namespace>(timeout: TimeSpan.FromMinutes(60),
            onEvent: OnEvent,
            onError: OnError,
            onClose: OnClosed,
            cancellationToken: stoppingToken))
            {
                this._logger.LogCritical("=== on watch Done ===");
                var ctrlc = new ManualResetEventSlim(false);
                ctrlc.Wait(CancellationToken.None);
            }

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

        private void OnEvent(WatchEventType arg1, V1Namespace @namespace)
        {
            _logger.LogInformation("OnEvent" + @namespace.Name());
            this.ReportHealthy("received new namespace" + @namespace.Name());

            if (arg1 == WatchEventType.Added || arg1 == WatchEventType.Modified)
            {
                var dtons = _mapper.Map<V1Namespace, NamespaceV1>(@namespace);
                redisDic.Add(@dtons);
            }
            else if (arg1 == WatchEventType.Deleted)
            {
                var dtons = _mapper.Map<V1Namespace, NamespaceV1>(@namespace);
                redisDic.Remove(@dtons);
            }
        }

        private void OnError(Exception ex)
        {
            this._logger.LogError("===on watch Exception : " + ex.Message);
            this.ReportUnhealthy("Error " + ex.Message);
        }

        private void OnClosed()
        {
            _logger.LogInformation("OnClosed TODO: retry the connection");
            var utc = DateTime.UtcNow.ToString();
            var howlongran = (DateTime.UtcNow - lastrestart);

            this._logger.LogError("===on watch Connection  Closed after " + howlongran.TotalMinutes.ToString() + ":" + howlongran.Seconds.ToString() + " min:sec : re-running delay 30 seconds " + utc);

            Task.Delay(TimeSpan.FromSeconds(30)).Wait();
            lastrestart = DateTime.UtcNow;
            this._logger.LogError("=== on watch Restarting Now.... ===" + lastrestart.ToString());
            executingTask = Task.Factory.StartNew(() => namespaceWatchStarter(cancellationToken), TaskCreationOptions.LongRunning);
        }



        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Namespace Watch Hosted Service is stopping.");
            await base.StopAsync(cancellationToken);
        }

    }
}