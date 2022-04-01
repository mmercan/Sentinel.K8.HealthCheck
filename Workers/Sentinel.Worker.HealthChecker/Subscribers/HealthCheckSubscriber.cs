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
            if (executingTask.IsCompleted)
            {
                return executingTask;
            }
            return Task.CompletedTask;
        }

        private void SubscribeQueue()
        {
            // try
            // {
            _logger.LogInformation("HealthCheckSubscriber: Connected to bus");

            _bus.PubSub.SubscribeAsync<HealthCheckResourceV1>(_configuration["queue:healthcheck"], Handler);

            _logger.LogInformation("HealthCheckSubscriber: Listening on topic " + _configuration["queue:healthcheck"]);

            _ResetEvent.Wait();
            // }
            // catch (Exception ex)
            // {
            //     this.ReportUnhealthy(ex.Message);
            //     _logger.LogError("HealthCheckSubscriber: Exception: " + ex.Message);
            // }
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
                var results = await _isAliveAndWelldownloader.DownloadAsync(healthcheck.RelatedService, healthcheck);
                this.saveToMongo(healthcheck, results);

            }
            _logger.LogInformation("HealthCheckSubscriber: Handler Received an item : " + healthcheck.Key + " Serevice Found: " + serviceFound + " service name: " + serviceName);
            // _ResetEvent.Set();
        }

        private void saveToMongo(HealthCheckResourceV1 healthcheck, List<IsAliveAndWellResult> results)
        {
            List<IsAliveAndWellResultTimeSerie> timeSeries = new List<IsAliveAndWellResultTimeSerie>();
            var items = _isAliveAndWellRepoTimeSeries.Items;
            foreach (var item in results)
            {
                IsAliveAndWellResultTimeSerie timeSerie = new IsAliveAndWellResultTimeSerie();
                // timeSerie.Id = Guid.NewGuid().ToString();
                timeSerie.Metadata = new IsAliveAndWellResultTimeSerieMetadata();
                timeSerie.Metadata.Namespace = healthcheck.RelatedService?.Namespace;
                timeSerie.Metadata.ServiceName = healthcheck.RelatedService?.Name;
                timeSerie.Metadata.CheckedUrl = item.CheckedUrl;
                timeSerie.ResultDetailId = item.Id;
                timeSerie.Status = item.Status;
                timeSerie.IsSuccessStatusCode = item.IsSuccessStatusCode;
                if (item.CheckedAt == DateTime.MinValue)
                {
                    timeSerie.CheckedAt = DateTime.UtcNow;
                }
                else
                {
                    timeSerie.CheckedAt = item.CheckedAt.ToUniversalTime();
                }

                timeSeries.Add(timeSerie);
            }
            var ids = string.Join(",", results.Select(x => x.Id).ToList());
            _logger.LogInformation("{resultsCount} IsAliveAndWellResult adding to Mongo. {ids}", results.Count().ToString(), ids);
            items.InsertMany(timeSeries);
            _isAliveAndWellRepo.Items.InsertMany(results);
            _logger.LogInformation("{resultsCount} IsAliveAndWellResult added to Mongo.", results.Count().ToString());
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