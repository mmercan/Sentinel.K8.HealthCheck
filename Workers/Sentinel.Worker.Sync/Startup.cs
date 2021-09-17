using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sentinel.K8s;
using k8s;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Turquoise.HealthChecks.Common.CheckCaller;
using System.Net.Http.Headers;
using Sentinel.Common.CustomFeatureFilter;
using Sentinel.Common.HttpClientHelpers;
using Microsoft.FeatureManagement;
using Quartz;
using Microsoft.FeatureManagement.FeatureFilters;
using Sentinel.Worker.Sync.JobSchedules;
using CrystalQuartz.Application;
using CrystalQuartz.AspNetCore;
using EasyNetQ;
using StackExchange.Redis;

namespace Sentinel.Worker.Sync
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


            if (Configuration["RunOnCluster"] == "true") { services.AddSingleton<KubernetesClientConfiguration>(KubernetesClientConfiguration.InClusterConfig()); }
            else { services.AddSingleton<KubernetesClientConfiguration>(KubernetesClientConfiguration.BuildConfigFromConfigFile()); }

            services.AddSingleton<IKubernetesClient, KubernetesClient>();
            services.AddSingleton<KubernetesClient>();

            services.Configure<KestrelServerOptions>(options => { options.AllowSynchronousIO = true; });
            services.Configure<IISServerOptions>(options => { options.AllowSynchronousIO = true; });

            services.AddSingleton<IServiceCollection>(services);
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddAutoMapper(typeof(Startup).Assembly, typeof(Sentinel.K8s.KubernetesClient).Assembly, typeof(Sentinel.Models.CRDs.HealthCheckResource).Assembly);

            services.AddHttpClient<HealthCheckReportDownloaderService>("HealthCheckReportDownloader", options =>
            {
                // options.BaseAddress = new Uri(Configuration["CrmConnection:ServiceUrl"] + "api/data/v8.2/");
                options.Timeout = new TimeSpan(0, 2, 0);
                options.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
                options.DefaultRequestHeaders.Add("OData-Version", "4.0");
                options.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            })
            .AddPolicyHandler(HttpClientHelpers.GetRetryPolicy())
            .AddPolicyHandler(HttpClientHelpers.GetCircuitBreakerPolicy());

            services.AddHttpContextAccessor();

            services.AddFeatureManagement()
            .AddFeatureFilter<PercentageFilter>()
            .AddFeatureFilter<HeadersFeatureFilter>();

            services.AddHealthChecks();

            services.Configure<QuartzOptions>(Configuration.GetSection("Quartz"));
            services.Configure<QuartzOptions>(options =>
            {
                options.Scheduling.IgnoreDuplicates = true; // default: false
                options.Scheduling.OverWriteExistingData = true; // default: true
            });


            services.AddQuartz(q =>
            {

                q.SchedulerId = "scheduler-sync";
                q.UseMicrosoftDependencyInjectionJobFactory();

                q.UseSimpleTypeLoader();
                q.UseInMemoryStore();
                q.UseDefaultThreadPool(tp => { tp.MaxConcurrency = 10; });

                if (Configuration["Schedules:NamespaceScheduler:enabled"] != null
                 && Configuration["Schedules:NamespaceScheduler:enabled"] == "true")
                {
                    q.ScheduleJob<NamespaceSchedulerJob>(trigger => trigger
                    .WithIdentity("NamespaceSchedulerJob")
                    .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(5)))
                    .WithCronSchedule(Configuration["Schedules:NamespaceScheduler:schedule"])
                    .WithDescription("Namespaces sync trigger configured run in Cron"));
                }

                if (Configuration["Schedules:ServiceScheduler:enabled"] != null
                 && Configuration["Schedules:ServiceScheduler:enabled"] == "true")
                {
                    q.ScheduleJob<ServiceSchedulerJob>(trigger => trigger
                    .WithIdentity("ServiceSchedulerJob")
                    .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(10)))
                    .WithCronSchedule(Configuration["Schedules:ServiceScheduler:schedule"])
                    .WithDescription("Service sync trigger configured run in Cron"));
                }

                if (Configuration["Schedules:DeploymentScheduler:enabled"] != null
                 && Configuration["Schedules:DeploymentScheduler:enabled"] == "true")
                {
                    q.ScheduleJob<DeploymentSchedulerJob>(trigger => trigger
                    .WithIdentity("DeploymentSchedulerJob")
                    .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(15)))
                    .WithCronSchedule(Configuration["Schedules:DeploymentScheduler:schedule"])
                    .WithDescription("Service sync trigger configured run in Cron"));
                }

                if (Configuration["Schedules:HealthCheckScheduler:enabled"] != null
                 && Configuration["Schedules:HealthCheckScheduler:enabled"] == "true")
                {
                    q.ScheduleJob<HealthCheckSchedulerJob>(trigger => trigger
                    .WithIdentity("HealthCheckSchedulerJob")
                    .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(20)))
                    .WithCronSchedule(Configuration["Schedules:HealthCheckScheduler:schedule"])
                    .WithDescription("Service sync trigger configured run in Cron"));
                }
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

            // services.AddStackExchangeRedisCache(options =>
            // {
            //     options.Configuration = Configuration["RedisConnection"];
            //     options.InstanceName = "ApiComms";
            //  });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ISchedulerFactory schedulerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }


            var options = new CrystalQuartzOptions
            {
                ErrorDetectionOptions = new CrystalQuartz.Application.ErrorDetectionOptions
                { VerbosityLevel = ErrorVerbosityLevel.Detailed }
                //JobDataMapDisplayOptions 
            };

            var scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();
            app.UseCrystalQuartz(() => scheduler, options);

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
