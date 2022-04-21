using CrystalQuartz.Application;
using CrystalQuartz.AspNetCore;
using Quartz;
using Sentinel.Common.Middlewares;
using Sentinel.K8s;
using Sentinel.Scheduler;
using Sentinel.Scheduler.Quartz;

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

            // Add services to the container.
            builder.Services.AddServiceDefinitions(
                builder.Configuration, typeof(IK8sLibAssemblyMarker),
                typeof(Sentinel.Worker.Core.Program)
                );
            builder.Services.AddAuthentication();
            builder.Services.AddAuthorization();
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

            app.UseEndpointDefinitions();

            app.UseRouting();

            app.UseAuthorization();

            var options = new CrystalQuartzOptions
            {
                ErrorDetectionOptions = new CrystalQuartz.Application.ErrorDetectionOptions
                { VerbosityLevel = ErrorVerbosityLevel.Detailed }
            };

            var schedulerFactory = app.Services.GetService<ISchedulerFactory>();
            var scheduler = schedulerFactory?.GetScheduler().GetAwaiter().GetResult();
            app.UseCrystalQuartz(() => scheduler, options);

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