using Sentinel.Common.Middlewares;
using Sentinel.Models.HealthCheck;

namespace Sentinel.Worker.HealthChecker.Modules
{
    public class MongoDBModule : IModule
    {
        public void MapEndpoints(WebApplication app)
        {

        }



        public void RegisterServices(IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddMongoTimeSeriesRepo<IsAliveAndWellResultTimeSerie>(
                configuration["Mongodb:ConnectionString"],
                configuration["Mongodb:DatabaseName"],
                configuration["Mongodb:HealthCheckResultTimeSeries"],
                p => p.Id,
                p => p.CheckedAt,
                p => p.Metadata
            );

            services.AddMongoRepo<IsAliveAndWellResult>(
                configuration["Mongodb:ConnectionString"],
                configuration["Mongodb:DatabaseName"],
                "HealthCheckResultSummary",
                p => p.Id
            );
        }
    }
}