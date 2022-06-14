using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Sentinel.Common.Middlewares
{
    public interface IModule
    {
        void RegisterServices(IServiceCollection services, ConfigurationManager configuration);
        void MapEndpoints(WebApplication app);

    }
}