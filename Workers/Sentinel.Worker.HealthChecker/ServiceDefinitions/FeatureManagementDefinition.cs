using Sentinel.Common.Middlewares;
using Sentinel.Models.HealthCheck;

namespace Sentinel.Worker.HealthChecker.ServiceDefinitions
{
    public class FeatureManagementDefinition : IEndpointDefinition
    {
        public void DefineEndpoints(WebApplication app)
        {

        }



        public void DefineServices(IServiceCollection services, ConfigurationManager configuration)
        {
            //   services.AddFeatureManagement()
            // .AddFeatureFilter<PercentageFilter>()
            // .AddFeatureFilter<HeadersFeatureFilter>();
        }
    }
}