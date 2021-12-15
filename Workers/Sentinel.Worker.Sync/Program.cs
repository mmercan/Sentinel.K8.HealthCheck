


using System.Net.Http.Headers;
using CrystalQuartz.Application;
using CrystalQuartz.AspNetCore;
using k8s;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
using Quartz;
using Sentinel.Common;
using Sentinel.Common.CustomFeatureFilter;
using Sentinel.Common.HttpClientHelpers;
using Sentinel.Common.Middlewares;
using Sentinel.K8s;
using Sentinel.Scheduler.Extensions;
using Sentinel.Worker.Sync.JobSchedules;
using Sentinel.Worker.Sync.Watchers;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
using Turquoise.HealthChecks.Common;
using Turquoise.HealthChecks.Common.CheckCaller;
using Turquoise.HealthChecks.Common.Checks;

namespace Sentinel.Worker.Sync
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            if (builder.Configuration["RunOnCluster"] == "true") { builder.Services.AddSingleton<KubernetesClientConfiguration>(KubernetesClientConfiguration.InClusterConfig()); }
            else { builder.Services.AddSingleton<KubernetesClientConfiguration>(KubernetesClientConfiguration.BuildConfigFromConfigFile()); }

            builder.Services.AddSingleton<IKubernetesClient, KubernetesClient>();
            builder.Services.AddSingleton<KubernetesClient>();

            builder.Services.Configure<KestrelServerOptions>(options => { options.AllowSynchronousIO = true; });
            builder.Services.Configure<IISServerOptions>(options => { options.AllowSynchronousIO = true; });

            builder.Services.AddSingleton<IServiceCollection>(builder.Services);
            builder.Services.AddSingleton<IConfiguration>(builder.Configuration);
            builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(Sentinel.K8s.KubernetesClient).Assembly, typeof(Sentinel.Models.CRDs.HealthCheckResource).Assembly);



            builder.Services.AddHttpContextAccessor();

            builder.Services.AddFeatureManagement()
            .AddFeatureFilter<PercentageFilter>()
            .AddFeatureFilter<HeadersFeatureFilter>();

            builder.Services.AddHealthChecks()
                .AddSystemInfoCheck()
            //    .AddConfigurationChecker(builder.Configuration);
            // .AddRedisHealthCheck(builder.Configuration["RedisConnection"])
             .AddConfigurationChecker(builder.Configuration);
            // .AddRabbitMQHealthCheckWithDiIBus();


            builder.Logging.AddSerilog();

            builder.Services.Configure<QuartzOptions>(builder.Configuration.GetSection("Quartz"));
            builder.Services.Configure<QuartzOptions>(options =>
            {
                options.Scheduling.IgnoreDuplicates = true; // default: false
                options.Scheduling.OverWriteExistingData = true; // default: true
            });


            builder.Services.AddQuartz(q =>
            {
                q.SchedulerId = builder.Environment.ApplicationName;
                q.UseMicrosoftDependencyInjectionJobFactory();
                q.UseSimpleTypeLoader();
                q.UseInMemoryStore();
                q.UseDefaultThreadPool(tp => { tp.MaxConcurrency = 10; });

                q.AddSchedulerJob<NamespaceSchedulerJob>(
                    builder.Configuration.GetSection("Schedules:NamespaceScheduler"), 5);

                q.AddSchedulerJob<ServiceSchedulerJob>(
                    builder.Configuration.GetSection("Schedules:ServiceScheduler"), 10);

                q.AddSchedulerJob<DeploymentSchedulerJob>(
                    builder.Configuration.GetSection("Schedules:DeploymentScheduler"), 15);

                q.AddSchedulerJob<HealthCheckSchedulerJob>(
                    builder.Configuration.GetSection("Schedules:HealthCheckScheduler"), 20);

                q.AddSchedulerJob<DeploymentScalersShedulerJob>(
                    builder.Configuration.GetSection("Schedules:DeploymentScalerScheduler"), 25);
            });

            builder.Services.AddQuartzServer(options =>
            {
                options.WaitForJobsToComplete = true;
                options.StartDelay = TimeSpan.FromSeconds(2);
            });

            builder.Services.AddSingleton<IConnectionMultiplexer>((ctx) =>
            {
                return ConnectionMultiplexer.Connect(builder.Configuration["RedisConnection"]);
            });


            if (builder.Configuration["Watchers:NamespaceWatcher:enabled"] != null
            && builder.Configuration["Watchers:NamespaceWatcher:enabled"] == "true")
            {
                builder.Services.AddHostedService<NamespaceWatcherJob>();
            }

            if (builder.Configuration["Watchers:DeploymentWatcher:enabled"] != null
            && builder.Configuration["Watchers:DeploymentWatcher:enabled"] == "true")
            {
                builder.Services.AddHostedService<DeploymentWatcherJob>();
            }


            // Configure
            // ##################################
            var app = builder.Build();

            var schedulerFactory = app.Services.GetService<ISchedulerFactory>();


            if (app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            var logger = LoggerHelper.ConfigureLogger("Sentinel.Worker.Sync",
                app.Environment.EnvironmentName, app.Configuration, LogEventLevel.Debug);

            //  loggerFactory.AddSerilog();
            Log.Logger = logger.CreateLogger();
            app.UseExceptionLogger();

            var options = new CrystalQuartzOptions
            {
                ErrorDetectionOptions = new CrystalQuartz.Application.ErrorDetectionOptions
                { VerbosityLevel = ErrorVerbosityLevel.Detailed }
            };


            var scheduler = schedulerFactory?.GetScheduler().GetAwaiter().GetResult();
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

            app.Run();

        }
    }
}
