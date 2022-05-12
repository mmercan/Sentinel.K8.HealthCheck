using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Sentinel.Common
{
    public abstract class BackgroundServiceWithHealthCheck : BackgroundService
    {
        protected Task executingTask;
        public DateTime LastRestart = DateTime.UtcNow;
        protected readonly ILogger<BackgroundServiceWithHealthCheck> _logger;
        protected readonly BackgroundServiceHealthCheck bgHealthCheck = default!;
        protected TimeSpan? timeout;
        protected Timer _timer = default!;
        protected bool isTriggered { get; set; } = true;
        protected ManualResetEventSlim _ResetEvent = new ManualResetEventSlim(false);
        protected virtual string appName
        {
            get { return this.GetType().Name; }
        }

        private int _heathCheckTimeoutTotalMinutes { get; set; } = default!;
        protected int HeathCheckTimeoutTotalMinutes
        {
            get { return _heathCheckTimeoutTotalMinutes; }
            set
            {
                _heathCheckTimeoutTotalMinutes = value;
                if (value > 0)
                {
                    timeout = TimeSpan.FromMinutes(value);
                }
            }
        }
        protected BackgroundServiceWithHealthCheck(ILogger<BackgroundServiceWithHealthCheck> logger,
        IOptions<HealthCheckServiceOptions> hcoptions, int HeathCheckTimeoutTotalMinutes = 5)
        {
            _logger = logger;
            if (hcoptions.Value != null)
            {
                bgHealthCheck = new BackgroundServiceHealthCheck();
                var registration = new HealthCheckRegistration(appName, bgHealthCheck, null, null);
                hcoptions.Value.Registrations.Add(registration);
                ReportHealthy(appName + " initialized");
            }
            this.HeathCheckTimeoutTotalMinutes = HeathCheckTimeoutTotalMinutes;
            executingTask = Task.CompletedTask;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            executingTask = Task.Factory.StartNew(async () => await Execute(stoppingToken), TaskCreationOptions.LongRunning);
            Task.Factory.StartNew(new Action(() =>
            {
                if (timeout.HasValue)
                {
                    _timer = new Timer(new TimerCallback(HealthCheckFailIfQueueNotUsed), null, TimeSpan.Zero, timeout.Value);
                }
            }), TaskCreationOptions.LongRunning);
            if (executingTask.IsCompleted) { return executingTask; }
            return Task.CompletedTask;
        }


        protected abstract Task Execute(CancellationToken stoppingToken);

        protected void HealthCheckFailIfQueueNotUsed(object? state)
        {
            if (!isTriggered && timeout.HasValue)
            {
                ReportUnhealthy(timeout.Value.ToString(@"dd\.hh\:mm\:ss") + "  have passed without receiving any message from the queue");
            }
            isTriggered = false;
        }
        public void ReportHealthy(string message = "") => bgHealthCheck.ReportHealthy(message);
        public void ReportUnhealthy(string message = "") => bgHealthCheck.ReportUnhealthy(message);
        public void ReportDegraded(string message = "") => bgHealthCheck.ReportDegraded(message);

        public override void Dispose()
        {
            _ResetEvent.Dispose();
            _timer?.Dispose();
            base.Dispose();
        }

    }

}