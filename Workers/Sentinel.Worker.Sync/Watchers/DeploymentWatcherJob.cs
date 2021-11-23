using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper.Configuration;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Sentinel.K8s;
using System.Linq;
using StackExchange.Redis;
using Sentinel.Redis;
using Sentinel.Models.K8sDTOs;
using AutoMapper;
using Sentinel.Common;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Sentinel.Worker.Sync.Watchers
{
    public class DeploymentWatcherJob : BackgroundServiceWithHealthCheck
    {
        private readonly IKubernetesClient _k8sService;
        private readonly IMapper _mapper;
        private readonly RedisDictionary<DeploymentV1> redisDic;
        private Task executingTask;
        private CancellationToken cancellationToken;

        public DeploymentWatcherJob(
            ILogger<DeploymentWatcherJob> logger,
            IKubernetesClient k8sService,
            IConnectionMultiplexer redisMultiplexer,
            IMapper mapper,
            IOptions<HealthCheckServiceOptions> hcoptions) : base(logger, hcoptions)
        {
            _k8sService = k8sService;
            _mapper = mapper;
            redisDic = new RedisDictionary<DeploymentV1>(redisMultiplexer, _logger, "DeploymentWatch");

            executingTask = Task.CompletedTask;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            cancellationToken = stoppingToken;
            executingTask = Task.Factory.StartNew(() => deployWatchStarter(stoppingToken), TaskCreationOptions.LongRunning);
            if (executingTask.IsCompleted)
            {
                return executingTask;
            }
            return Task.CompletedTask;
        }


        private void deployWatchStarter(CancellationToken stoppingToken)
        {
            this.ReportHealthy();
            this._logger.LogCritical("Watch Started");

            using (_k8sService.Watch<V1Deployment>(timeout: TimeSpan.FromMinutes(60),
                onEvent: watcher,
                onError: OnError,
                onClose: OnClosed,
                cancellationToken: stoppingToken))
            {
                this._logger.LogCritical("=== on watch Done ===");
                var ctrlc = new ManualResetEventSlim(false);
                ctrlc.Wait(CancellationToken.None);
            }
        }

        private void watcher(WatchEventType type, V1Deployment item)
        {
            _logger.LogInformation("OnEvent" + item.Name());
            this.ReportHealthy("received new Deployment" + item.Name());

            if (type == WatchEventType.Added || type == WatchEventType.Modified)
            {
                var dtons = _mapper.Map<V1Deployment, DeploymentV1>(item);
                redisDic.Add(@dtons);
            }
            else if (type == WatchEventType.Deleted)
            {
                var dtons = _mapper.Map<V1Deployment, DeploymentV1>(item);
                redisDic.Remove(@dtons);
            }
        }

        private void OnError(Exception ex)
        {
            this._logger.LogError("===on watch Exception : DeploymentWatcherSyncService " + ex.Message);
            this.ReportUnhealthy("Error " + ex.Message);
        }

        private void OnClosed()
        {
            var utc = DateTime.UtcNow.ToString();
            var howlongran = (DateTime.UtcNow - lastrestart);

            this._logger.LogError("===on watch DeploymentWatcherJob Connection  Closed after " + howlongran.TotalMinutes.ToString() + ":" + howlongran.Seconds.ToString() + " min:sec : re-running delay 30 seconds " + utc);

            Task.Delay(TimeSpan.FromSeconds(30)).Wait();
            lastrestart = DateTime.UtcNow;
            this._logger.LogError("=== on watch Restarting DeploymentWatch Now.... ===" + lastrestart.ToString());
            executingTask = Task.Factory.StartNew(() => deployWatchStarter(cancellationToken), TaskCreationOptions.LongRunning);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deployment Watch Hosted Service is stopping.");
            await base.StopAsync(cancellationToken);
        }

    }

}