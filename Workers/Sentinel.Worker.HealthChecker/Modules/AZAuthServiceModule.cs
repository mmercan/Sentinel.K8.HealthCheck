using System.IdentityModel.Tokens.Jwt;
using Microsoft.Identity.Web;
using Sentinel.Common.AuthServices;
using Sentinel.Common.Middlewares;
using Sentinel.Models.HealthCheck;

namespace Sentinel.Worker.HealthChecker.Modules
{
    public class AZAuthServiceModule : IModule
    {
        public void MapEndpoints(WebApplication app)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }



        public void RegisterServices(IServiceCollection services, ConfigurationManager configuration)
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