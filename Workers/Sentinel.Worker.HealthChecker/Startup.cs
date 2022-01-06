using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using EasyNetQ;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Microsoft.FeatureManagement.FeatureFilters;
using Sentinel.Common;
using Sentinel.Common.AuthServices;
using Sentinel.Common.CustomFeatureFilter;
using Sentinel.Common.HttpClientHelpers;
using Sentinel.Common.HttpClientServices;
using Sentinel.Common.Middlewares;
using Sentinel.Worker.HealthChecker.Subscribers;
using Serilog;
using Serilog.Events;
using StackExchange.Redis;
using Turquoise.HealthChecks.Common;
using Turquoise.HealthChecks.Common.CheckCaller;
using Turquoise.HealthChecks.Common.Checks;
using Turquoise.HealthChecks.RabbitMQ;
using Turquoise.HealthChecks.Redis;
using Turquoise.HealthChecks.Mongo;
using Microsoft.Identity.Web;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Sentinel.Worker.HealthChecker
{
    public class Startup
    {

        public IConfiguration Configuration { get; }
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(options => { options.AllowSynchronousIO = true; });
            services.Configure<IISServerOptions>(options => { options.AllowSynchronousIO = true; });

            services.AddSingleton<IServiceCollection>(services);
            services.AddSingleton<IConfiguration>(Configuration);

            services.AddMemoryCache();
            services.Configure<AZAuthServiceSettings>(Configuration.GetSection("AzureAd"));
            services.AddSingleton<AZAuthService>();

            services.AddMicrosoftIdentityWebApiAuthentication(Configuration)
                .EnableTokenAcquisitionToCallDownstreamApi()
                .AddMicrosoftGraph(Configuration.GetSection("DownstreamApi"))
                .AddInMemoryTokenCaches();

            // IdentityModelEventSource.ShowPII = true;
            JwtSecurityTokenHandler.DefaultMapInboundClaims = false;
            // services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            // { options.TokenValidationParameters.RoleClaimType = "roles"; });

            services.AddSingleton<DownloadJsonService>();
            services.AddSingleton<IsAliveAndWellHealthCheckDownloader>();

            services.AddAutoMapper(typeof(Startup).Assembly, typeof(Sentinel.K8s.KubernetesClient).Assembly, typeof(Sentinel.Models.CRDs.HealthCheckResource).Assembly);

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

            services.AddFeatureManagement()
            .AddFeatureFilter<PercentageFilter>()
            .AddFeatureFilter<HeadersFeatureFilter>();

            services.AddHealthChecks()
                .AddSystemInfoCheck()
                .AddRedisHealthCheck(Configuration["RedisConnection"])
                .AddMongoHealthCheck(Configuration["Mongodb:ConnectionString"])
                .AddRabbitMQHealthCheckWithDiIBus()
                .AddConfigurationChecker(Configuration);

            services.AddSingleton<EasyNetQ.IBus>((ctx) =>
            {
                return RabbitHutch.CreateBus(Configuration["RabbitMQConnection"]);
            });

            services.AddSingleton<IConnectionMultiplexer>((ctx) =>
            {
                return ConnectionMultiplexer.Connect(Configuration["RedisConnection"]);
            });

            services.AddHostedService<HealthCheckSubscriber>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseExceptionLogger();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseHealthChecks("/Health/IsAliveAndWell", new HealthCheckOptions() { });
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

                endpoints.MapGet("/user", async context =>
                {
                    var q = context.User.IsInRole("Admin");
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync("{\"IsAlive\":true}");
                }).RequireAuthorization();
            });
        }
    }
}
