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
            //ThreadPool.ThreadCount;
            ThreadPool.SetMinThreads(100, 100);
            services.AddSingleton<IConnectionMultiplexer>((ctx) =>
             {
                 // var configurationOptions = new ConfigurationOptions
                 // {
                 //     EndPoints = { configuration.GetValue<string>("redis:host") },
                 //     Password = configuration.GetValue<string>("redis:password"),
                 //     AllowAdmin = true,
                 //     ConnectTimeout = 5000,
                 //     SyncTimeout = 5000,
                 //     AbortOnConnectFail = false,
                 //     ConnectRetry = 5,
                 //     KeepAlive = 180,
                 //     DefaultDatabase = configuration.GetValue<int>("redis:database"),
                 //     ClientName = "Sentinel.Redis"
                 // };

                 return ConnectionMultiplexer.Connect(configuration["RedisConnection"]);
             });
        }
    }
}