using System;
using System.Threading;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sentinel.Common;
using Sentinel.Models.K8sDTOs;

namespace Sentinel.Worker.HealthChecker.Subscribers
{
    public class HealthCheckSubscriber : BackgroundServiceWithHealthCheck
    {
        private Task executingTask;
        private readonly EasyNetQ.IBus _bus;
        private readonly IConfiguration _configuration;
        private readonly string timezone;
        private ManualResetEventSlim _ResetEvent = new ManualResetEventSlim(false);

        public HealthCheckSubscriber(
            ILogger<HealthCheckSubscriber> logger,
            IBus bus,
            IOptions<HealthCheckServiceOptions> hcoptions,
            IConfiguration configuration
            ) : base(logger, hcoptions)
        {
            _bus = bus;
            _configuration = configuration;

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
            try
            {

                _logger.LogInformation("Connected to bus");
                //_bus.SubscribeAsync<ServiceV1>(_configuration["queue:servicev1"], Handler); //, x => x.WithTopic("product.*"));
                _bus.PubSub.SubscribeAsync<HealthCheckResourceV1>(_configuration["queue:healthcheck"], Handler);

                _logger.LogInformation("Listening on topic " + _configuration["queue:servicev1"]);
                // HealthcheckQueueSubscriberStats.SetIsqueueSubscriberStarted(true);
                _ResetEvent.Wait();
            }
            catch (Exception ex)
            {
                this.ReportUnhealthy(ex.Message);
                _logger.LogError("Exception: " + ex.Message);
            }
        }

        private Task Handler(HealthCheckResourceV1 healthcheck)
        {
            this.ReportHealthy();
            bool serviceFound = false;
            string serviceName = "";
            if (healthcheck.RelatedService != null)
            {
                serviceFound = true;
                serviceName = healthcheck.RelatedService.NameandNamespace;
            }
            _logger.LogInformation(" Handler Received an item : " + healthcheck.Key + " Serevice Found: " + serviceFound + "service name: " + serviceName);
            return Task.CompletedTask;
        }
    }
}