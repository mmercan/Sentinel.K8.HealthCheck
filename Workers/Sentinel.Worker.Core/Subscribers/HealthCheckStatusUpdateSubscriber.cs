using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Sentinel.K8s.K8sClients;
using Sentinel.Models.HealthCheck;
using Sentinel.Models.Scheduler;
using Sentinel.PubSub.BackgroundServices;

namespace Sentinel.Worker.Core.Subscribers
{
    [RabbitMQSubscribe(Name = "healthcheck.status.update", TopicConfigurationSection = "queue:healthcheckStatusUpdate", TimeoutTotalMinutes = 3, Description = "healthcheck.status.update", Enabled = true)]
    public class HealthCheckStatusUpdateSubscriber : SubscribeBackgroundService<IsAliveAndWellResultListWithHealthCheck>
    {
        private readonly K8sGeneralService _k8sGeneralService;

        public HealthCheckStatusUpdateSubscriber(
                   IBus bus, IConfiguration configuration, ILogger<SubscribeBackgroundService<IsAliveAndWellResultListWithHealthCheck>> logger,
                   IOptions<HealthCheckServiceOptions> hcoptions,
                   K8sGeneralService k8sGeneralService
                   )
                   : base(bus, configuration, logger, hcoptions)
        {
            _k8sGeneralService = k8sGeneralService;
        }

        protected override async Task Handler(IsAliveAndWellResultListWithHealthCheck healthcheck)
        {
            var Name = healthcheck.HealthCheck.Name;
            var Namespace = healthcheck.HealthCheck.Namespace;
            var HealthCheckUid = healthcheck.HealthCheck.Uid;
            var checkedUrl = healthcheck.IsAliveAndWellResult.CheckedUrl;
            var status = healthcheck.IsAliveAndWellResult.Status;
            var qq = healthcheck.IsAliveAndWellResult.Result;
            if (healthcheck.IsAliveAndWellResult.Result != null)
            {
                try
                {
                    JObject json = JObject.Parse(healthcheck.IsAliveAndWellResult.Result);
                    if (json != null)
                    {
                        var internalstatus = json["status"]?.ToString();
                        if (internalstatus != null)
                        {
                            status = internalstatus;
                        }
                    }
                }
                catch
                {

                }
            }
            await _k8sGeneralService.HealthCheckResourceClient.UpdateStartusAsync(Name, Namespace, status, checkedUrl, DateTime.UtcNow);

            // await _k8sGeneralService.EventClient.CountUpOrCreateEvent(
            //      Namespace, Name, HealthCheckUid,  HealthCheckResourceV1.ApiVersion,
            //               serviceResourceVersion, message, type: "Normal");
            _logger.LogInformation("HealthCheckStatusUpdateSubscriber: Received status update for " + Name + " in namespace " + Namespace + " with status " + status + ". Check Url : " + healthcheck.IsAliveAndWellResult.CheckedUrl);
        }
    }
}