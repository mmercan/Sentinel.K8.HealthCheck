using System;
using EasyNetQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement.FeatureFilters;
using Microsoft.FeatureManagement;
using Quartz;
using Sentinel.Common.CustomFeatureFilter;
using Sentinel.Models.K8sDTOs;
using Sentinel.Scheduler;
using Sentinel.Scheduler.Extensions;
using Sentinel.Worker.Scheduler.JobSchedules;
using StackExchange.Redis;
using Serilog;
using Serilog.Events;
using Microsoft.Extensions.Logging;
using CrystalQuartz.Application;
using CrystalQuartz.AspNetCore;
using Sentinel.Common.Middlewares;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Turquoise.HealthChecks.Common;
using Sentinel.Worker.Scheduler.Schedules;
using Turquoise.HealthChecks.Common.Checks;
using Turquoise.HealthChecks.Redis;
using Turquoise.HealthChecks.RabbitMQ;
using Sentinel.Common;

namespace Sentinel.Worker.Scheduler
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(options => { options.AllowSynchronousIO = true; });
            services.Configure<IISServerOptions>(options => { options.AllowSynchronousIO = true; });

            services.AddSingleton<IServiceCollection>(services);
            services.AddSingleton<IConfiguration>(Configuration);

            services.AddAutoMapper(typeof(Startup).Assembly, typeof(Sentinel.K8s.KubernetesClient).Assembly, typeof(Sentinel.Models.CRDs.HealthCheckResource).Assembly);

            services.AddSingleton<SchedulerRepository<HealthCheckResourceV1>>();
            services.AddSingleton<SchedulerRepositoryFeeder<HealthCheckResourceV1>>();

            services.AddHttpContextAccessor();

            services.AddFeatureManagement()
            .AddFeatureFilter<PercentageFilter>()
            .AddFeatureFilter<HeadersFeatureFilter>();

            services.AddHealthChecks()
                .AddSystemInfoCheck()
                .AddRedisHealthCheck(Configuration["RedisConnection"])
                .AddRabbitMQHealthCheckWithDiIBus()
                .AddConfigurationChecker(Configuration);

            services.Configure<QuartzOptions>(Configuration.GetSection("Quartz"));
            services.Configure<QuartzOptions>(options =>
            {
                options.Scheduling.IgnoreDuplicates = true; // default: false
                options.Scheduling.OverWriteExistingData = true; // default: true
            });


            services.AddQuartz(q =>
            {
                q.SchedulerId = "scheduler-scheduler";
                q.UseMicrosoftDependencyInjectionJobFactory();

                q.UseSimpleTypeLoader();
                q.UseInMemoryStore();
                q.UseDefaultThreadPool(tp => { tp.MaxConcurrency = 10; });

                q.AddSchedulerJob<HealthCheckResourceFeederJob>(
                    Configuration.GetSection("Schedules:HealthCheckResourceFeederJob"), 5);

            });

            services.AddQuartzServer(options =>
            {
                options.WaitForJobsToComplete = true;
                options.StartDelay = TimeSpan.FromSeconds(2);
            });

            services.AddSingleton<EasyNetQ.IBus>((ctx) =>
            {
                return RabbitHutch.CreateBus(Configuration["RabbitMQConnection"]);
            });

            services.AddSingleton<IConnectionMultiplexer>((ctx) =>
            {
                return ConnectionMultiplexer.Connect(Configuration["RedisConnection"]);
            });

            services.AddHostedService<BusScheduler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ISchedulerFactory schedulerFactory, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseExceptionLogger();

            var options = new CrystalQuartzOptions
            {
                ErrorDetectionOptions = new CrystalQuartz.Application.ErrorDetectionOptions
                { VerbosityLevel = ErrorVerbosityLevel.Detailed }
            };


            var scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();
            app.UseCrystalQuartz(() => scheduler, options);

            app.UseRouting();

            app.UseHealthChecks("/Health/IsAliveAndWell", new HealthCheckOptions()
            {
                ResponseWriter = WriteResponses.WriteListResponse,
            });


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"IsAlive\":true}");
                });
            });
        }
    }
}
