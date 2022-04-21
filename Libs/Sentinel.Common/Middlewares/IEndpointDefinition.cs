using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Sentinel.Common.Middlewares
{
    public interface IEndpointDefinition
    {
        void DefineServices(IServiceCollection services, ConfigurationManager  configuration);
        void DefineEndpoints(WebApplication app);

    }
}