using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentinel.Common.Middlewares;
using EasyNetQ;

namespace Libs.Sentinel.PubSub.Modules
{
    public class RabbitMQServiceModule : IModule
    {
        public void MapEndpoints(WebApplication app)
        {

        }

        public void RegisterServices(IServiceCollection services, ConfigurationManager configuration)
        {
            services.AddSingleton<EasyNetQ.IBus>((ctx) =>
            {
                return RabbitHutch.CreateBus(configuration["RabbitMQConnection"]);
            });
        }
    }
}