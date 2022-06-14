using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentinel.Common.Middlewares;

namespace Libs.Sentinel.Common.ServiceDefinitions
{
    public class ServiceCollectionModule : IModule
    {
        public void MapEndpoints(WebApplication app)
        {
            app.UseExceptionLogger();
        }

        public void RegisterServices(IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddSingleton<IServiceCollection>(services);
            services.AddSingleton<IConfiguration>(configuration);
        }
    }
}