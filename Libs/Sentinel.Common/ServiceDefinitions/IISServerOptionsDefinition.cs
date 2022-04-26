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
    public class IISServerOptionsDefinition : IEndpointDefinition
    {
        public void DefineEndpoints(WebApplication app)
        {

        }

        public void DefineServices(IServiceCollection services, ConfigurationManager configuration)
        {
            services.Configure<KestrelServerOptions>(options => { options.AllowSynchronousIO = true; });
            services.Configure<IISServerOptions>(options => { options.AllowSynchronousIO = true; });
        }
    }
}