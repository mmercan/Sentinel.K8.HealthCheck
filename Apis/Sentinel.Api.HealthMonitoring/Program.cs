using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Turquoise.HealthChecks.Common;
using Turquoise.HealthChecks.Common.Checks;

namespace Sentinel.Worker.Sync
{
    public class Program
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorPages();

            builder.Services.AddHealthChecks()
                            .AddSystemInfoCheck()
                            // .AddRedisHealthCheck(Configuration["RedisConnection"])
                            // .AddRabbitMQHealthCheckWithDiIBus()
                            .AddConfigurationChecker(builder.Configuration);


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseHealthChecks("/Health/IsAliveAndWell", new HealthCheckOptions()
            {
                ResponseWriter = WriteResponses.WriteListResponse,
            });

            app.UseAuthorization();

            app.MapRazorPages();

            app.Run();
        }
    }
}