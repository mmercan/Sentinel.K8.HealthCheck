using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sentinel.Common;
using Sentinel.Common.HttpClientServices;
using Sentinel.Models.HealthCheck;
using Sentinel.Models.K8sDTOs;
using Sentinel.Mongo;

namespace Sentinel.Worker.HealthChecker.Subscribers
{
    public class HealthCheckSubscriber : BackgroundServiceWithHealthCheck
    {
        private Task executingTask;
        private readonly EasyNetQ.IBus _bus;
        private readonly IConfiguration _configuration;
        private readonly IsAliveAndWellHealthCheckDownloader _isAliveAndWelldownloader;
        private readonly MongoBaseRepo<IsAliveAndWellResult> _isAliveAndWellRepo;
        private readonly MongoBaseRepo<IsAliveAndWellResultTimeSerie> _isAliveAndWellRepoTimeSeries;
        private readonly string timezone;
        private ManualResetEventSlim _ResetEvent = new ManualResetEventSlim(false);

        public HealthCheckSubscriber(
            ILogger<HealthCheckSubscriber> logger,
            IBus bus,
            IOptions<HealthCheckServiceOptions> hcoptions,
            IConfiguration configuration,
            IsAliveAndWellHealthCheckDownloader isAliveAndWelldownloader,
            MongoBaseRepo<IsAliveAndWellResult> isAliveAndWellRepo,
            MongoBaseRepo<IsAliveAndWellResultTimeSerie> isAliveAndWellRepoTimeSeries
            ) : base(logger, hcoptions)
        {
            _bus = bus;
            _configuration = configuration;
            _isAliveAndWelldownloader = isAliveAndWelldownloader;
            _isAliveAndWellRepo = isAliveAndWellRepo;
            _isAliveAndWellRepoTimeSeries = isAliveAndWellRepoTimeSeries;

            if (!string.IsNullOrWhiteSpace(_configuration["timezone"]))
            {
                timezone = _configuration["timezone"];
            }
            else
            {
                timezone = "Australia/Melbourne";
            }
            executingTask = Task.CompletedTask;
        }




        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            executingTask = Task.Factory.StartNew(new Action(SubscribeQueue), TaskCreationOptions.LongRunning);
            if (executingTask.IsCompleted) { return executingTask; }
            return Task.CompletedTask;
        }

        private void SubscribeQueue()
        {
            try
            {
                _logger.LogInformation("HealthCheckSubscriber: Connected to bus");
                _bus.PubSub.SubscribeAsync<HealthCheckResourceV1>(_configuration["queue:healthcheck"], Handler);
                _logger.LogInformation("HealthCheckSubscriber: Listening on topic " + _configuration["queue:healthcheck"]);
                _ResetEvent.Wait();
            }
            catch (Exception ex)
            {
                this.ReportUnhealthy(ex.Message);
                _logger.LogError("HealthCheckSubscriber: Exception: " + ex.Message);
            }
        }

        private async Task Handler(HealthCheckResourceV1 healthcheck)
        {
            this.ReportHealthy();
            bool serviceFound = false;
            string serviceName = "";
            if (healthcheck.RelatedService != null)
            {
                serviceFound = true;
                serviceName = healthcheck.RelatedService.NameandNamespace;
                var result = await _isAliveAndWelldownloader.DownloadAsync(healthcheck.RelatedService, healthcheck);
                this.QueueHealthCheckK8sUpdate(healthcheck, result);
                await this.saveToMongo(healthcheck, result);
            }
            else
            {
                _logger.LogInformation("HealthCheckSubscriber: Handler Received an item but Related Service Not Found: " + healthcheck.Key + " Service Found: " + serviceFound + " service name: " + serviceName);
                await Task.CompletedTask;
            }
            _logger.LogInformation("HealthCheckSubscriber: Handler Received an item : " + healthcheck.Key + " Service Found: " + serviceFound + " service name: " + serviceName);
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

        private void OnClosed()
        {
            var utc = DateTime.UtcNow.ToString();
            var howlongran = (DateTime.UtcNow - lastrestart);

            this._logger.LogError("===on watch HealthCheckSubscriber Connection  Closed after " + howlongran.TotalMinutes.ToString() + ":" + howlongran.Seconds.ToString() + " min:sec : re-running delay 30 seconds " + utc);

            Task.Delay(TimeSpan.FromSeconds(30)).Wait();
            lastrestart = DateTime.UtcNow;
            this._logger.LogError("=== on watch Restarting HealthCheckSubscriber Now.... ===" + lastrestart.ToString());
            executingTask = Task.Factory.StartNew(new Action(SubscribeQueue), TaskCreationOptions.LongRunning);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("HealthCheckSubscriber Hosted Service is stopping.");
            await base.StopAsync(cancellationToken);
        }
    }
}