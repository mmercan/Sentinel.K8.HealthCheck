using CrystalQuartz.Application;
using CrystalQuartz.AspNetCore;
using Quartz;
using Sentinel.Common;
using Sentinel.Common.Middlewares;
using Sentinel.K8s;
using Sentinel.PubSub;
using Sentinel.Redis;
using Sentinel.Scheduler;
using Sentinel.Scheduler.Quartz;
using Serilog;
using Serilog.Events;

namespace Sentinel.Worker.Core
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            if (environment == null) { environment = "Development"; }
            var appname = System.AppDomain.CurrentDomain.FriendlyName;

            var builder = WebApplication.CreateBuilder(args);
            builder.Host.UseSerilogAuto(appname, environment, LogEventLevel.Debug);
            builder.Logging.AddSerilog();

            // Add services to the container.
            builder.Services.AddServiceDefinitions(
                builder.Configuration,
                typeof(ICommonLibAssemblyMarker),
                typeof(IK8sLibAssemblyMarker),
                typeof(IRedisLibAssemblyMarker),
                typeof(ISchedulerLibAssemblyMarker),
                typeof(IPubSubLibAssemblyMarker),
                typeof(Sentinel.Worker.Core.Program)
            );

            // builder.Services.AddAuthentication();
            // builder.Services.AddAuthorization();
            builder.Services.AddAutoMapper(typeof(Program).Assembly, typeof(Sentinel.K8s.KubernetesClient).Assembly, typeof(Sentinel.Models.CRDs.HealthCheckResource).Assembly);

            builder.Services.AddQuartzJobs(builder.Configuration, typeof(Program));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            // if (!app.Environment.IsDevelopment())
            // {
            //     app.UseExceptionHandler("/Error");
            //     // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //     app.UseHsts();
            // }

            app.UseRouting();
            app.UseEndpointDefinitions();
            app.Run();
        }
    }
}