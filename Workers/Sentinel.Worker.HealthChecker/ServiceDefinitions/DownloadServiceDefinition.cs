using System.Net.Http.Headers;
using Sentinel.Common.HttpClientHelpers;
using Sentinel.Common.HttpClientServices;
using Sentinel.Common.Middlewares;
using Sentinel.Models.HealthCheck;
using Turquoise.HealthChecks.Common.CheckCaller;

namespace Sentinel.Worker.HealthChecker.ServiceDefinitions
{
    public class DownloadServiceDefinition : IEndpointDefinition
    {
        public void DefineEndpoints(WebApplication app)
        {

        }



        public void DefineServices(IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddSingleton<DownloadService>();
            services.AddSingleton<IsAliveAndWellHealthCheckDownloader>();

            services.AddHttpClient<HealthCheckReportDownloaderService>("HealthCheckReportDownloader", options =>
           {
               // options.BaseAddress = new Uri(Configuration["CrmConnection:ServiceUrl"] + "api/data/v8.2/");
               options.Timeout = new TimeSpan(0, 2, 0);
               options.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
               options.DefaultRequestHeaders.Add("OData-Version", "4.0");
               options.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
           })
           .AddPolicyHandler(HttpClientHelpers.GetRetryPolicy())
           .AddPolicyHandler(HttpClientHelpers.GetCircuitBreakerPolicy());

            services.AddHttpContextAccessor();
        }
    }
}