using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Sentinel.Common;
using EasyNetQ;
using Sentinel.K8s.K8sClients;
using Sentinel.Models.K8sDTOs;
using Sentinel.Models.HealthCheck;

namespace Sentinel.Worker.Sync.Subscribers
{
    public class HealthCheckStatusUpdateSubscriber : BackgroundServiceWithHealthCheck
    {
        private Task executingTask;
        private ManualResetEventSlim _ResetEvent = new ManualResetEventSlim(false);
        private readonly IBus _bus;
        private readonly IConfiguration _configuration;
        private readonly K8sGeneralService _k8sGeneralService;

        public HealthCheckStatusUpdateSubscriber(
            ILogger<BackgroundServiceWithHealthCheck> logger,
            IBus bus,
            IConfiguration configuration,
            K8sGeneralService k8sGeneralService,
            IOptions<HealthCheckServiceOptions> hcoptions) : base(logger, hcoptions)
        {
            _bus = bus;
            _configuration = configuration;
            _k8sGeneralService = k8sGeneralService;
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
                _bus.PubSub.SubscribeAsync<IsAliveAndWellResultListWithHealthCheck>(_configuration["queue:healthcheckStatusUpdate"], Handler);
                _logger.LogInformation("HealthCheckSubscriber: Listening on topic " + _configuration["queue:healthcheckStatusUpdate"]);
                _ResetEvent.Wait();
            }
            catch (Exception ex)
            {
                this.ReportUnhealthy(ex.Message);
                _logger.LogError("HealthCheckSubscriber: Exception: " + ex.Message);
            }
        }


        private async Task Handler(IsAliveAndWellResultListWithHealthCheck healthcheck)
        {
            var Name = healthcheck.HealthCheck.Name;
            var Namespace = healthcheck.HealthCheck.Namespace;
            var HealthCheckUid = healthcheck.HealthCheck.Uid;

            var status = healthcheck.IsAliveAndWellResult.Status;
            await _k8sGeneralService.HealthCheckResourceClient.UpdateStartusAsync(Name, Namespace, status, DateTime.UtcNow);

            // await _k8sGeneralService.EventClient.CountUpOrCreateEvent(
            //      Namespace, Name, HealthCheckUid,  HealthCheckResourceV1.ApiVersion,
            //               serviceResourceVersion, message, type: "Normal");
            _logger.LogInformation("HealthCheckSubscriber: Received status update for " + Name + " in namespace " + Namespace + " with status " + status + ". Check Url : " + healthcheck.IsAliveAndWellResult.CheckedUrl);
            this.ReportHealthy();
        }

        private void FindStatus(IsAliveAndWellResult result)
        {

            switch (result.StatusCode)
            {
                case 200:
                    result.IsSuccessStatusCode = true;
                    break;
                case 404:
                    result.IsSuccessStatusCode = false;
                    break;
                case 500:
                    result.IsSuccessStatusCode = false;
                    break;
                default:
                    result.IsSuccessStatusCode = false;
                    break;
            }

        }
    }
}