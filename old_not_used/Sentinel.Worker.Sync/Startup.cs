// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Builder;
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.AspNetCore.Http;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.DependencyInjection;
// using Microsoft.Extensions.Hosting;
// using Sentinel.K8s;
// using k8s;
// using Microsoft.AspNetCore.Server.Kestrel.Core;
// using Turquoise.HealthChecks.Common.CheckCaller;
// using System.Net.Http.Headers;
// using Sentinel.Common.CustomFeatureFilter;
// using Sentinel.Common.HttpClientHelpers;
// using Microsoft.FeatureManagement;
// using Quartz;
// using Microsoft.FeatureManagement.FeatureFilters;
// using Sentinel.Worker.Sync.JobSchedules;
// using CrystalQuartz.Application;
// using CrystalQuartz.AspNetCore;
// using EasyNetQ;
// using StackExchange.Redis;
// using Serilog;
// using Serilog.Events;
// using Microsoft.Extensions.Logging;
// using Microsoft.AspNetCore.Diagnostics.HealthChecks;
// using Turquoise.HealthChecks.Common;
// using Sentinel.Worker.Sync.Watchers;
// using Sentinel.Scheduler.Extensions;
// using Sentinel.Common.Middlewares;
// using Sentinel.Common;

// namespace Sentinel.Worker.Sync
// {
//     public class Startup
//     {
//         public IConfiguration Configuration { get; }
//         public Startup(IConfiguration configuration)
//         {
//             Configuration = configuration;
//         }
//         // This method gets called by the runtime. Use this method to add services to the container.
//         // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
//         public void ConfigureServices(IServiceCollection services)
//         {
//             if (Configuration["RunOnCluster"] == "true") { services.AddSingleton<KubernetesClientConfiguration>(KubernetesClientConfiguration.InClusterConfig()); }
//             else { services.AddSingleton<KubernetesClientConfiguration>(KubernetesClientConfiguration.BuildConfigFromConfigFile()); }

//             services.AddSingleton<IKubernetesClient, KubernetesClient>();
//             services.AddSingleton<KubernetesClient>();

//             services.Configure<KestrelServerOptions>(options => { options.AllowSynchronousIO = true; });
//             services.Configure<IISServerOptions>(options => { options.AllowSynchronousIO = true; });

//             services.AddSingleton<IServiceCollection>(services);
//             services.AddSingleton<IConfiguration>(Configuration);
//             services.AddAutoMapper(typeof(Startup).Assembly, typeof(Sentinel.K8s.KubernetesClient).Assembly, typeof(Sentinel.Models.CRDs.HealthCheckResource).Assembly);

//             services.AddHttpClient<HealthCheckReportDownloaderService>("HealthCheckReportDownloader", options =>
//             {
//                 // options.BaseAddress = new Uri(Configuration["CrmConnection:ServiceUrl"] + "api/data/v8.2/");
//                 options.Timeout = new TimeSpan(0, 2, 0);
//                 options.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
//                 options.DefaultRequestHeaders.Add("OData-Version", "4.0");
//                 options.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

//             })
//             .AddPolicyHandler(HttpClientHelpers.GetRetryPolicy())
//             .AddPolicyHandler(HttpClientHelpers.GetCircuitBreakerPolicy());

//             services.AddHttpContextAccessor();

//             services.AddFeatureManagement()
//             .AddFeatureFilter<PercentageFilter>()
//             .AddFeatureFilter<HeadersFeatureFilter>();

//             services.AddHealthChecks();

//             services.Configure<QuartzOptions>(Configuration.GetSection("Quartz"));
//             services.Configure<QuartzOptions>(options =>
//             {
//                 options.Scheduling.IgnoreDuplicates = true; // default: false
//                 options.Scheduling.OverWriteExistingData = true; // default: true
//             });


//             services.AddQuartz(q =>
//             {

//                 q.SchedulerId = "scheduler-sync";
//                 q.UseMicrosoftDependencyInjectionJobFactory();

//                 q.UseSimpleTypeLoader();
//                 q.UseInMemoryStore();
//                 q.UseDefaultThreadPool(tp => { tp.MaxConcurrency = 10; });

//                 q.AddSchedulerJob<NamespaceSchedulerJob>(
//                     Configuration.GetSection("Schedules:NamespaceScheduler"), 5);

//                 q.AddSchedulerJob<ServiceSchedulerJob>(
//                     Configuration.GetSection("Schedules:ServiceScheduler"), 10);

//                 q.AddSchedulerJob<DeploymentSchedulerJob>(
//                     Configuration.GetSection("Schedules:DeploymentScheduler"), 15);

//                 q.AddSchedulerJob<HealthCheckSchedulerJob>(
//                     Configuration.GetSection("Schedules:HealthCheckScheduler"), 20);

//                 q.AddSchedulerJob<DeploymentScalersShedulerJob>(
//                     Configuration.GetSection("Schedules:DeploymentScalerScheduler"), 25);



//             });

//             services.AddQuartzServer(options =>
//             {
//                 options.WaitForJobsToComplete = true;
//                 options.StartDelay = TimeSpan.FromSeconds(2);
//             });

//             services.AddSingleton<IConnectionMultiplexer>((ctx) =>
//             {
//                 return ConnectionMultiplexer.Connect(Configuration["RedisConnection"]);
//             });


//             if (Configuration["Watchers:NamespaceWatcher:enabled"] != null
//             && Configuration["Watchers:NamespaceWatcher:enabled"] == "true")
//             {
//                 services.AddHostedService<NamespaceWatcherJob>();
//             }

//             if (Configuration["Watchers:DeploymentWatcher:enabled"] != null
//             && Configuration["Watchers:DeploymentWatcher:enabled"] == "true")
//             {
//                 services.AddHostedService<DeploymentWatcherJob>();
//             }

//         }

//         // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
//         public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ISchedulerFactory schedulerFactory, ILoggerFactory loggerFactory)
//         {
//             if (env.IsDevelopment())
//             {
//                 app.UseDeveloperExceptionPage();
//             }


//             var logger = LoggerHelper.ConfigureLogger("Sentinel.Worker.Sync",
//                 env.EnvironmentName, Configuration, LogEventLevel.Debug);

//             loggerFactory.AddSerilog();
//             Log.Logger = logger.CreateLogger();
//             app.UseExceptionLogger();


//             var options = new CrystalQuartzOptions
//             {
//                 ErrorDetectionOptions = new CrystalQuartz.Application.ErrorDetectionOptions
//                 { VerbosityLevel = ErrorVerbosityLevel.Detailed }
//             };


//             var scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();
//             app.UseCrystalQuartz(() => scheduler, options);

//             app.UseRouting();

//             app.UseHealthChecks("/Health/IsAliveAndWell", new HealthCheckOptions()
//             {
//                 ResponseWriter = WriteResponses.WriteListResponse,
//             });

//             app.UseEndpoints(endpoints =>
//             {
//                 endpoints.MapGet("/", async context =>
//                 {
//                     context.Response.ContentType = "application/json";
//                     await context.Response.WriteAsync("{\"IsAlive\":true}");
//                 });
//             });
//         }
//     }
// }
