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

namespace Sentinel.Worker.Sync.Watchers
{

    public class DeploymentWatcherSyncService : BackgroundService
    {
        private ILogger<DeploymentWatcherSyncService> logger;
        private IConfiguration configuration;
        private Task executingTask;
        private DateTime lastrestart = DateTime.UtcNow;
        private readonly IKubernetesClient k8sService;

        private readonly IDistributedCache cache;

        public DeploymentWatcherSyncService(
            ILogger<DeploymentWatcherSyncService> logger,
            IConfiguration configuration,
            IKubernetesClient k8sService,
            IDistributedCache cache
            // StackExchange.Redis.IRedis redis
            )
        {
            this.logger = logger;
            this.configuration = configuration;
            this.k8sService = k8sService;
            this.cache = cache;
            // redis.Ping();
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
            var deploylistResp = k8sService.ApiClient.ListNamespacedDeploymentWithHttpMessagesAsync("", watch: true);
            var podlistResp = k8sService.ApiClient.ListNamespacedPodWithHttpMessagesAsync("", watch: true);

            // var podlistResp123 = k8sService.client.ListPodForAllNamespacesAsync(watch: true);

            this.logger.LogCritical("Watch Started");
            //     using (podlistResp.Watch<V1Pod, V1PodList>((type, item) =>
            //    {
            //        this.logger.LogCritical("==on watch event==");
            //        this.logger.LogCritical(type.ToString());
            //        this.logger.LogCritical(item.Metadata.Name);
            //        this.logger.LogCritical("===on watch event===");
            //    }))
            //     {
            //         var ctrlc = new ManualResetEventSlim(false);
            //         ctrlc.Wait();
            //     }



            using (deploylistResp.Watch<V1Deployment, V1DeploymentList>(watcher, onError: OnError, onClosed: OnClosed))
            {
                this.logger.LogCritical("=== on watch Done ===");
                var ctrlc = new ManualResetEventSlim(false);
                ctrlc.Wait();
            }


        }

        private void watcher(WatchEventType type, V1Deployment item)
        {
            this.logger.LogCritical("==on watch event==");
            this.logger.LogCritical(type.ToString());
            this.logger.LogCritical(item.Metadata.Name);
            this.logger.LogCritical("===on watch event===");
            SavetoCache(item).Wait();
        }

        private void OnError(Exception ex)
        {
            this.logger.LogError("===on watch Exception : " + ex.Message);
        }

        private void OnClosed()
        {
            var utc = DateTime.UtcNow.ToString();
            var howlongran = (DateTime.UtcNow - lastrestart);

            this.logger.LogError("===on watch Connection  Closed after " + howlongran.TotalMinutes.ToString() + ":" + howlongran.Seconds.ToString() + " min:sec : re-running delay 30 seconds " + utc);


            Task.Delay(TimeSpan.FromSeconds(30)).Wait();
            lastrestart = DateTime.UtcNow;
            this.logger.LogError("=== on watch Restarting Now.... ===" + lastrestart.ToString());
            executingTask = Task.Factory.StartNew(new Action(deployWatchStarter), TaskCreationOptions.LongRunning);
        }



        private async Task SavetoCache(V1Deployment item)
        {
            string key = item.Metadata.Namespace() + ":" + item.Name();
            await SavetoCache(key, item);
        }
        private async Task SavetoCache(string key, V1Deployment data)
        {

            // cache.Get()
            var datajson = data.ToJSON();
            byte[] databyte = Encoding.UTF8.GetBytes(datajson);
            // var options = new DistributedCacheEntryOptions()
            //     .SetSlidingExpiration(TimeSpan.FromSeconds(20));
            await cache.SetAsync(key, databyte);
        }

    }

}