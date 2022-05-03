using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Sentinel.Common.Middlewares;
using Turquoise.HealthChecks.Common;
using Turquoise.HealthChecks.Common.Checks;
using Turquoise.HealthChecks.RabbitMQ;
using Turquoise.HealthChecks.Mongo;

namespace Sentinel.Worker.HealthChecker.ServiceDefinitions
{
    public class HealthCheckServiceDefinition : IEndpointDefinition
    {
        public void DefineEndpoints(WebApplication app)
        {
            if (app.Services.GetService<HealthCheckService>() != null)
            {
                app.UseHealthChecks("/Health/IsAliveAndWell", new HealthCheckOptions()
                {
                    ResponseWriter = WriteResponses.WriteListResponse,
                });

                app.UseHealthChecksWithAuth("/Health/IsAliveAndWellDetail", new HealthCheckOptions()
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



        public void DefineServices(IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddHealthChecks()
            .AddSystemInfoCheck()
            .AddMongoHealthCheck(configuration["Mongodb:ConnectionString"])
            .AddConfigurationChecker(configuration)
            .AddRabbitMQHealthCheckWithDiIBus();
        }
    }
}