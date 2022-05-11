using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Sentinel.Common;
using Sentinel.Common.HttpClientServices;
using Sentinel.Models.HealthCheck;
using Sentinel.Models.K8sDTOs;
using Sentinel.Models.Scheduler;
using Sentinel.Mongo;
using Sentinel.PubSub.BackgroundServices;

namespace Sentinel.Worker.HealthChecker.Subscribers
{
    [RabbitMQSubscribe(Name = "HealthChecker", TopicConfigurationSection = "queue:healthcheck", TimeoutTotalMinutes = 3, Description = "HealthChecker", Enabled = true)]
    public class HealthCheckSubscriber : SubscribeBackgroundService<IScheduledTaskItem>
    {
        private readonly IsAliveAndWellHealthCheckDownloader _isAliveAndWelldownloader;
        private readonly MongoBaseRepo<IsAliveAndWellResult> _isAliveAndWellRepo;
        private readonly MongoBaseRepo<IsAliveAndWellResultTimeSerie> _isAliveAndWellRepoTimeSeries;
        public HealthCheckSubscriber(
            IBus bus, IConfiguration configuration, ILogger<SubscribeBackgroundService<IScheduledTaskItem>> logger,
            IsAliveAndWellHealthCheckDownloader isAliveAndWelldownloader,
            MongoBaseRepo<IsAliveAndWellResult> isAliveAndWellRepo,
            MongoBaseRepo<IsAliveAndWellResultTimeSerie> isAliveAndWellRepoTimeSeries,
            IOptions<HealthCheckServiceOptions> hcoptions)
            : base(bus, configuration, logger, hcoptions)
        {
            _isAliveAndWelldownloader = isAliveAndWelldownloader;
            _isAliveAndWellRepo = isAliveAndWellRepo;
            _isAliveAndWellRepoTimeSeries = isAliveAndWellRepoTimeSeries;
        }

        protected override async Task Handler(IScheduledTaskItem scheduledItem)
        {
            if (scheduledItem is not HealthCheckResourceV1)
            {
                _logger.LogInformation("HealthCheckSubscriber: Received unknown scheduled item with Key {key} and type {type}", scheduledItem.Key, scheduledItem.GetType().ToString());
                return;
            }

            HealthCheckResourceV1? healthcheck = scheduledItem as HealthCheckResourceV1;

            bool serviceFound = false;
            string serviceName = "";
            if (healthcheck?.RelatedService == null)
            {
                _logger.LogInformation("HealthCheckSubscriber: Handler Received an item but Related Service Not Found: {key} Service Found: {serviceFound} service name: {serviceName}",
                healthcheck?.Key, serviceFound, serviceName);
                return;
            }

            serviceFound = true;
            serviceName = healthcheck.RelatedService.NameandNamespace;
            var result = await _isAliveAndWelldownloader.DownloadAsync(healthcheck.RelatedService, healthcheck);
            if (result == null)
            {
                _logger.LogInformation("HealthCheckSubscriber: Handler Received an item but AliveAndWelldownloader result is null");
                return;
            }

            this.QueueHealthCheckK8sUpdate(healthcheck, result);
            await this.saveToMongo(healthcheck, result);
            _logger.LogInformation("HealthCheckSubscriber: Handler Received an item : {Key} Service Found: {serviceFound}  service name: {serviceName}", healthcheck.Key, serviceFound, serviceName);
            // _ResetEvent.Set();
        }

        private async Task saveToMongo(HealthCheckResourceV1 healthcheck, IsAliveAndWellResult result)
        {
            var items = _isAliveAndWellRepoTimeSeries.Items;
            var timeSerie = IsAliveAndWellResultTimeSerie.FromIsAliveAndWellResult(healthcheck, result);
            var ids = result.Id;
            _logger.LogInformation("IsAliveAndWellResult adding to Mongo. {ids}", ids);
            await _isAliveAndWellRepoTimeSeries.AddAsync(timeSerie);
            await _isAliveAndWellRepo.AddAsync(result);
            _logger.LogInformation("IsAliveAndWellResult added to Mongo. {ids}", ids);
        }

        private void QueueHealthCheckK8sUpdate(HealthCheckResourceV1 healthcheck, IsAliveAndWellResult result)
        {
            IsAliveAndWellResultListWithHealthCheck check = new IsAliveAndWellResultListWithHealthCheck();
            check.HealthCheck = healthcheck;
            check.IsAliveAndWellResult = result;
            _bus.PubSub.PublishAsync(check, _configuration["queue:healthcheckStatusUpdate"]).ContinueWith(task =>
             {
                 if (task.IsCompleted && !task.IsFaulted)
                 {
                     _logger.LogInformation("Task Added to RabbitMQ {healthcheckStatusUpdate} {Key} ", _configuration["queue:healthcheckStatusUpdate"], check.HealthCheck.Key);
                 }
                 if (task.IsFaulted)
                 {
                     _logger.LogError("BusScheduler Failed : {Exception} ", task.Exception.MessageWithInnerException());
                     var constring = _configuration["RabbitMQConnection"];
                     _logger.LogDebug("RabbitMQConnection {RabbitMQConnection}", constring);
                 }
             });
        }



        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("HealthCheckSubscriber Hosted Service is stopping.");
            await base.StopAsync(cancellationToken);
        }
    }
}