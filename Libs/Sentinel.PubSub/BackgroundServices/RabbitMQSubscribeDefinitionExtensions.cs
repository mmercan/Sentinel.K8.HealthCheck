using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Sentinel.Common.Middlewares;

namespace Sentinel.PubSub.BackgroundServices
{
    public static class RabbitMQSubscribeDefinitionExtensions
    {
        public static void AddRabbitMQSubscribeDefinitions(this IServiceCollection services, IConfiguration configuration, params Type[] scanMarkers)
        {

            var rabbitMQSubscribeDefinitions = new List<Type>();
            foreach (var marker in scanMarkers)
            {
                rabbitMQSubscribeDefinitions.AddRange(marker.Assembly.ExportedTypes
                .Where(t => typeof(SubscribeBackgroundService).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract)
                //.Select(Activator.CreateInstance).Cast<SubscribeBackgroundService>()
                );
            }
            foreach (var subscribeBackgroundService in rabbitMQSubscribeDefinitions)
            {
                services.AddHostedServices(subscribeBackgroundService);
            }
            //  services.AddSingleton(rabbitMQSubscribeDefinitions as IReadOnlyCollection<SubscribeBackgroundService>);
        }
    }

}
