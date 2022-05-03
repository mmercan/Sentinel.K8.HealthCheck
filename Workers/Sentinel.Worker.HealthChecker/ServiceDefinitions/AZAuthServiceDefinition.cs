using System.IdentityModel.Tokens.Jwt;
using Microsoft.Identity.Web;
using Sentinel.Common.AuthServices;
using Sentinel.Common.Middlewares;
using Sentinel.Models.HealthCheck;

namespace Sentinel.Worker.HealthChecker.ServiceDefinitions
{
    public class AZAuthServiceDefinition : IEndpointDefinition
    {
        public void DefineEndpoints(WebApplication app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }



        public void DefineServices(IServiceCollection services, ConfigurationManager configuration)
        {
            services.Configure<AZAuthServiceSettings>(configuration.GetSection("AzureAd"));
            services.AddSingleton<AZAuthService>();

            services.AddMicrosoftIdentityWebApiAuthentication(configuration)
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddMicrosoftGraph(configuration.GetSection("DownstreamApi"))
                .AddInMemoryTokenCaches();

            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
        }
    }
}