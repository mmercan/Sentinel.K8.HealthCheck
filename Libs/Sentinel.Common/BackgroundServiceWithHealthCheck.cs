using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Sentinel.Common
{
    public abstract class BackgroundServiceWithHealthCheck : BackgroundService
    {
        protected readonly IServiceProvider _serviceProvider;
        protected readonly ILogger<BackgroundServiceWithHealthCheck> _logger;
        protected readonly IServiceCollection _services;

        public WorkerWitness Witness { get; set; }

        public BackgroundServiceWithHealthCheck(IServiceProvider serviceProvider, ILogger<BackgroundServiceWithHealthCheck> logger, IServiceCollection services)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _services = services;
            Witness = new WorkerWitness();


            services.Configure<HealthCheckServiceOptions>(options =>
            {
                var name = this.GetType().ToString();
                var registration = new HealthCheckRegistration(name, new BackgroundServiceHealthCheck(this), null, null);
                options.Registrations.Add(registration);
            });


        }

    }



    public class WorkerWitness
    {
        private DateTime _lastProcessTime;
        private int _count;

        public DateTime GetLastProcessTime()
        {
            return _lastProcessTime;
        }

        public int GetCount()
        {
            return _count;
        }
        public void SetProcessTime()
        {
            _lastProcessTime = DateTime.UtcNow;
            _count++;
        }
    }
}