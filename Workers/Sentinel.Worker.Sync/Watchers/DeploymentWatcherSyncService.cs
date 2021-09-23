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
using Sentinel.Worker.Sync.RedisHelpers;

namespace Sentinel.Worker.Sync.Watchers
{

    public class DeploymentWatcherSyncService : BackgroundService
    {
        private readonly ILogger<DeploymentWatcherSyncService> _logger;
        private readonly IKubernetesClient _k8sService;
        private readonly IDatabase _redisDatabase;
        private Task executingTask;
        private DateTime lastrestart = DateTime.UtcNow;

        public DeploymentWatcherSyncService(
            ILogger<DeploymentWatcherSyncService> logger,
            IKubernetesClient k8sService,
            IConnectionMultiplexer redisMultiplexer
            )
        {
            this._logger = logger;
            this._k8sService = k8sService;
            this._redisDatabase = redisMultiplexer.GetDatabase();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            executingTask = Task.Factory.StartNew(new Action(deployWatchStarter), TaskCreationOptions.LongRunning);
            if (executingTask.IsCompleted)
            {
                return executingTask;
            }
            return Task.CompletedTask;
        }


        private void deployWatchStarter()
        {
            var deploylistResp = _k8sService.ApiClient.ListNamespacedDeploymentWithHttpMessagesAsync("", watch: true);
            this._logger.LogCritical("Watch Started");

            using (deploylistResp.Watch<V1Deployment, V1DeploymentList>(
                onEvent: watcher,
                onError: OnError,
                onClosed: OnClosed))
            {
                this._logger.LogCritical("=== on watch Done ===");
                var ctrlc = new ManualResetEventSlim(false);
                ctrlc.Wait();
            }
        }

        private void watcher(WatchEventType type, V1Deployment item)
        {
            this._logger.LogCritical("==on watch event==");
            this._logger.LogCritical(type.ToString());
            this._logger.LogCritical(item.Metadata.Name);
            this._logger.LogCritical("===on watch event===");
            SavetoCache(item).Wait();
        }

        private void OnError(Exception ex)
        {
            this._logger.LogError("===on watch Exception : " + ex.Message);
        }

        private void OnClosed()
        {
            var utc = DateTime.UtcNow.ToString();
            var howlongran = (DateTime.UtcNow - lastrestart);

            this._logger.LogError("===on watch Connection  Closed after " + howlongran.TotalMinutes.ToString() + ":" + howlongran.Seconds.ToString() + " min:sec : re-running delay 30 seconds " + utc);

            Task.Delay(TimeSpan.FromSeconds(30)).Wait();
            lastrestart = DateTime.UtcNow;
            this._logger.LogError("=== on watch Restarting Now.... ===" + lastrestart.ToString());
            executingTask = Task.Factory.StartNew(new Action(deployWatchStarter), TaskCreationOptions.LongRunning);
        }



        private async Task SavetoCache(V1Deployment item)
        {
            string key = item.Metadata.Namespace() + ":" + item.Name();
            await SavetoCache(key, item);
        }
        private async Task SavetoCache(string key, V1Deployment data)
        {
            // var datajson = data.ToJSON();
            // byte[] databyte = Encoding.UTF8.GetBytes(datajson);
            // var options = new DistributedCacheEntryOptions()
            //    .SetSlidingExpiration(TimeSpan.FromMinutes(20));

            await _redisDatabase.SetAsync(key, data);
        }

    }

}