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
    public abstract class SubscribeBackgroundService : BackgroundService
    {
        protected DateTime lastrestart = DateTime.UtcNow;
        protected Task executingTask;
        protected readonly string timezone;

        protected readonly EasyNetQ.IBus _bus;
        protected readonly IConfiguration _configuration;
        protected readonly ILogger<SubscribeBackgroundService> _logger;
        protected readonly BackgroundServiceHealthCheck bgHealthCheck = default!;

        protected virtual string appName
        {
            get { return this.GetType().Name; }
        }
        protected string topicName { get; set; } = default!;

        protected ManualResetEventSlim _ResetEvent = new ManualResetEventSlim(false);

        public SubscribeBackgroundService(
            IBus bus,
            IConfiguration configuration,
            ILogger<SubscribeBackgroundService> logger,
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
            if (executingTask.IsCompleted) { return executingTask; }
            return Task.CompletedTask;
        }

        private void SubscribeQueue()
        {
            try
            {
                _logger.LogInformation(appName + " : Connected to bus");
                _bus.PubSub.SubscribeAsync<IScheduledTaskItem>(topicName, Handler);
                _logger.LogInformation(appName + " : Listening on topic " + topicName);
                _ResetEvent.Wait();
            }
            catch (Exception ex)
            {
                this.ReportUnhealthy(ex.Message);
                _logger.LogError(appName + " : Exception: " + ex.Message);
            }
        }
        abstract protected Task Handler(IScheduledTaskItem healthcheckTask);


        private void getAttributeDetails()
        {
            Type t = this.GetType();
            // Get instance of the attribute.
            RabbitMQSubscribeAttribute rabbitMQAttribute =
                (RabbitMQSubscribeAttribute)Attribute.GetCustomAttribute(t, typeof(RabbitMQSubscribeAttribute));

            if (rabbitMQAttribute == null)
            {
                _logger.LogWarning("The attribute was not found.");
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
                        _logger.LogWarning("The attribute TopicConfigurationSection found. But the configuration section was not found. Config section: {section}"
                        , rabbitMQAttribute.TopicConfigurationSection);
                    }
                }
                if (topicName == null)
                {
                    topicName = rabbitMQAttribute.TopicName;
                }

                _logger.LogInformation("The Name Attribute is: {0}. Topic Name : {TopicName} Enabled : {Enabled}",
                 rabbitMQAttribute.Name, rabbitMQAttribute.TopicName, rabbitMQAttribute.Enabled);
                _logger.LogInformation("The Description Attribute is: {0}.", rabbitMQAttribute.Description);
            }
        }
        public void ReportHealthy(string message = "") => bgHealthCheck.ReportHealthy(message);
        public void ReportUnhealthy(string message = "") => bgHealthCheck.ReportUnhealthy(message);
        public void ReportDegraded(string message = "") => bgHealthCheck.ReportDegraded(message);
    }
}