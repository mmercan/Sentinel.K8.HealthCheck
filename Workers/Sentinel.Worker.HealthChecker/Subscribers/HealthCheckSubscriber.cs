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
                _logger.LogCritical("Connected to bus");
                //  _bus.SubscribeAsync<ServiceV1>(_configuration["queue:servicev1"], Handler); //, x => x.WithTopic("product.*"));
                _logger.LogCritical("Listening on topic " + _configuration["queue:servicev1"]);
                // HealthcheckQueueSubscriberStats.SetIsqueueSubscriberStarted(true);
                _ResetEvent.Wait();
            }
            catch (Exception ex)
            {
                // HealthcheckQueueSubscriberStats.SetIsqueueSubscriberStarted(false);
                _logger.LogError("Exception: " + ex.Message);
            }
        }

        private Task Handler(ServiceV1 service)
        {
            return Task.CompletedTask;
        }
    }
}