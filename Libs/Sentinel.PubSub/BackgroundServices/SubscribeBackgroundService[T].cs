using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sentinel.Common;
using Sentinel.Models.Scheduler;

namespace Sentinel.PubSub.BackgroundServices
{
    public abstract class SubscribeBackgroundService<T> : BackgroundService
    {
        protected DateTime lastrestart = DateTime.UtcNow;
        protected Task executingTask;
        protected readonly string timezone;
        protected readonly EasyNetQ.IBus _bus;
        protected readonly IConfiguration _configuration;
        protected readonly ILogger<SubscribeBackgroundService<T>> _logger;
        protected readonly BackgroundServiceHealthCheck bgHealthCheck = default!;
        protected string topicName { get; set; } = default!;
        protected TimeSpan? timeout;
        protected DateTime LastRun { get; set; } = DateTime.UtcNow;
        private Timer _timer;
        protected bool isTriggered { get; set; } = true;
        protected ManualResetEventSlim _ResetEvent = new ManualResetEventSlim(false);
        protected virtual string appName
        {
            get { return this.GetType().Name; }
        }

        public SubscribeBackgroundService(
            IBus bus,
            IConfiguration configuration,
            ILogger<SubscribeBackgroundService<T>> logger,
            IOptions<HealthCheckServiceOptions> hcoptions
            )
        {
            _bus = bus;
            _configuration = configuration;
            _logger = logger;

            if (hcoptions.Value != null)
            {
                var name = this.GetType().ToString();
                bgHealthCheck = new BackgroundServiceHealthCheck();
                var registration = new HealthCheckRegistration(name, bgHealthCheck, null, null);
                hcoptions.Value.Registrations.Add(registration);
                ReportHealthy(name + " initialized");
            }

            if (!string.IsNullOrWhiteSpace(_configuration["timezone"]))
            {
                timezone = _configuration["timezone"];
            }
            else
            {
                timezone = "Australia/Melbourne";
            }
            getAttributeDetails();
            executingTask = Task.CompletedTask;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            executingTask = Task.Factory.StartNew(new Action(SubscribeQueue), TaskCreationOptions.LongRunning);

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
        protected void HealthCheckFailIfQueueNotUsed(object state)
        {
            if (!isTriggered)
            {
                ReportUnhealthy(timeout.Value.ToString(@"dd\.hh\:mm\:ss") + "  have passed without receiving any message from the queue");
            }
            isTriggered = false;
        }
        private void SubscribeQueue()
        {
            try
            {
                _logger.LogInformation("{appName} : Connected to bus", appName);
                _bus.PubSub.SubscribeAsync<T>(topicName, HandlerPrivate);

                _logger.LogInformation("{appName} : Listening on topic {topicName} type of {TType}", appName, topicName, typeof(T).ToString());
                _ResetEvent.Wait();
            }
            catch (Exception ex)
            {
                this.ReportUnhealthy(ex.Message);
                _logger.LogError("{appName} : Exception: {Message}", appName, ex.Message);
            }
        }
        protected async Task HandlerPrivate(T healthcheckTask)
        {

            isTriggered = true;
            LastRun = DateTime.UtcNow;
            this.ReportHealthy("Queue is Receiving messages at " + LastRun.ToString("dd/MM/yyyy HH:mm:ss"));

            var item = healthcheckTask.ToJSON().Substring(0, 200).ToString();
            _logger.LogInformation("{appName} Received Type: {type}  Key: {key} on Topic: {topic}",
            appName, healthcheckTask.GetType().Name, item, topicName);
            await Handler(healthcheckTask);
        }

        abstract protected Task Handler(T scheduledItem);


        private void getAttributeDetails()
        {
            Type t = this.GetType();
            // Get instance of the attribute.
            RabbitMQSubscribeAttribute rabbitMQAttribute =
                (RabbitMQSubscribeAttribute)Attribute.GetCustomAttribute(t, typeof(RabbitMQSubscribeAttribute));

            if (rabbitMQAttribute == null)
            {
                _logger.LogWarning("RabbitMQSubscribeAttribute The attribute was not found. on {appName}", appName);
            }
            else
            {
                var AppName = t.Name;
                string SubscribeName = string.IsNullOrEmpty(rabbitMQAttribute.Name) ? AppName : AppName;

                if (rabbitMQAttribute.TopicConfigurationSection != null)
                {
                    if (_configuration[rabbitMQAttribute.TopicConfigurationSection] != null)
                    {
                        topicName = _configuration[rabbitMQAttribute.TopicConfigurationSection];
                    }
                    else
                    {
                        _logger.LogWarning("The attribute TopicConfigurationSection found. But the Topic was not found in Config section: {section}"
                        , rabbitMQAttribute.TopicConfigurationSection);
                    }
                }
                if (topicName == null)
                {
                    topicName = rabbitMQAttribute.TopicName;
                }
                if (rabbitMQAttribute.TimeoutTotalMinutes > 0)
                {
                    timeout = TimeSpan.FromMinutes(rabbitMQAttribute.TimeoutTotalMinutes);
                }
                _logger.LogInformation("The Name Attribute is: {0}. Topic Name : {TopicName} Enabled : {Enabled}",
                 rabbitMQAttribute.Name, topicName, rabbitMQAttribute.Enabled);
                _logger.LogInformation("The Description Attribute is: {0}.", rabbitMQAttribute.Description);
            }
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