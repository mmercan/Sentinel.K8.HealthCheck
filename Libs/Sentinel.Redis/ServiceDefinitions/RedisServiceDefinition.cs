using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentinel.Common.Middlewares;
using StackExchange.Redis;

namespace Sentinel.Redis.ServiceDefinitions
{
    public class RedisServiceDefinition : IEndpointDefinition
    {
        public void DefineEndpoints(WebApplication app)
        {

        }

        public void DefineServices(IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>((ctx) =>
             {
                 return ConnectionMultiplexer.Connect(configuration["RedisConnection"]);
             });
        }
    }
}