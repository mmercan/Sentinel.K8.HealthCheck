using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Sentinel.Models.Scheduler;
using Sentinel.PubSub.BackgroundServices;

namespace Sentinel.Worker.HealthChecker.Subscribers
{
    [RabbitMQSubscribe(Name = "HealthChecker", TopicName = "HealthChecker", TimeoutTotalMinutes = 3, Description = "HealthChecker", Enabled = true)]
    public class OtherSubs : SubscribeBackgroundService
    {
        public OtherSubs(IBus bus,
            IConfiguration configuration,
            ILogger<SubscribeBackgroundService> logger,
            IOptions<HealthCheckServiceOptions> hcoptions) : base(bus, configuration, logger, hcoptions)
        {

        }
        protected override Task Handler(IScheduledTaskItem healthcheckTask)
        {
            throw new NotImplementedException();
        }
    }
}