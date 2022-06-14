using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Libs.Sentinel.K8s;
using Libs.Sentinel.Scheduler;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Sentinel.Common.Middlewares;
using Sentinel.Models.K8sDTOs;
using Sentinel.Scheduler;
using Sentinel.Scheduler.Middlewares;
using Sentinel.Worker.Core.Feeder;
using Turquoise.HealthChecks.Common;
using Turquoise.HealthChecks.Common.Checks;
using Turquoise.HealthChecks.RabbitMQ;
using Turquoise.HealthChecks.Redis;

namespace Workers.Sentinel.Worker.Core.ServiceDefinitions
{
    public class RepositoryModule : IModule
    {
        public void MapEndpoints(WebApplication app)
        {

        }



        public void RegisterServices(IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddSingleton<K8MemoryRepository>();

            // services.AddSingleton<ScreenShotFeeder>();


            //  HealthCheck Feeder and Repository
            //services.AddSchedulerRedisRepositoryFeeder<HealthCheckResourceV1>(configuration.GetSection("Rediskey:HealthChecks"));
            // services.Configure<RedisKeyFeederOption<HealthCheckResourceV1>>(configuration.GetSection("Rediskey:HealthChecks"));
            services.AddSingleton<SchedulerRepository<HealthCheckResourceV1>>();
            // services.AddSingleton<SchedulerRedisRepositoryFeeder<HealthCheckResourceV1>>();

            // Scaler Feeder and Repository
            // services.AddSchedulerRedisRepositoryFeeder<DeploymentScalerResourceV1>(configuration.GetSection("Rediskey:DeploymentScalers"));
            //services.Configure<RedisKeyFeederOption<DeploymentScalerResourceV1>>(configuration.GetSection("Rediskey:DeploymentScalers"));
            services.AddSingleton<SchedulerRepository<DeploymentScalerResourceV1>>();
            //services.AddSingleton<SchedulerRedisRepositoryFeeder<DeploymentScalerResourceV1>>();
        }
    }
}