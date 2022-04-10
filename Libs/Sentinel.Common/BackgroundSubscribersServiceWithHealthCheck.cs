using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sentinel.Common
{
    public abstract class BackgroundSubscribersServiceWithHealthCheck : BackgroundServiceWithHealthCheck
    {

        private Task executingTask;
        private ManualResetEventSlim _ResetEvent = new ManualResetEventSlim(false);
        public BackgroundSubscribersServiceWithHealthCheck(ILogger<BackgroundServiceWithHealthCheck> logger, IOptions<HealthCheckServiceOptions> hcoptions) : base(logger, hcoptions)
        {
            executingTask = Task.CompletedTask;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            executingTask = Task.Factory.StartNew(new Action(SubscribeQueue), TaskCreationOptions.LongRunning);
            if (executingTask.IsCompleted) { return executingTask; }
            return Task.CompletedTask;
        }
        public abstract void SubscribeQueue();
    }
}