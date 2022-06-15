using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentinel.Common.Middlewares;

namespace Libs.Sentinel.Common.Modules
{
    public class HttpContextModule : IModule
    {
        public void MapEndpoints(WebApplication app)
        {

        }

        public void RegisterServices(IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddHttpContextAccessor();
        }
    }
}