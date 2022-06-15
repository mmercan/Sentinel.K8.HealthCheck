using Sentinel.Common.Middlewares;
using Sentinel.Models.HealthCheck;

namespace Sentinel.Worker.HealthChecker.Modules
{
    public class FeatureManagementModule : IModule
    {
        public void MapEndpoints(WebApplication app)
        {

        }



        public void RegisterServices(IServiceCollection services, ConfigurationManager configuration)
        {
            //   services.AddFeatureManagement()
            // .AddFeatureFilter<PercentageFilter>()
            // .AddFeatureFilter<HeadersFeatureFilter>();
        }
    }
}