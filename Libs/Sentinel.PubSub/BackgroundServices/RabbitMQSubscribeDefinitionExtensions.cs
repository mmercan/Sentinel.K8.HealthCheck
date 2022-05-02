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
                var items = marker.Assembly.ExportedTypes.Where(
                    t => typeof(SubscribeBackgroundService).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract
                    && Attribute.IsDefined(t, typeof(RabbitMQSubscribeAttribute))
                );
                if (items != null && items.Any())
                {
                    rabbitMQSubscribeDefinitions.AddRange(items);
                }
            }
            foreach (var subscribeBackgroundService in rabbitMQSubscribeDefinitions)
            {
                 RabbitMQSubscribeAttribute? rabbitAttr = subscribeBackgroundService.GetCustomAttributes(typeof(RabbitMQSubscribeAttribute), true).First() as RabbitMQSubscribeAttribute;
                    if (rabbitAttr != null)
                    {
                        bool enabled = rabbitAttr.Enabled;
                        if(enabled){
                            services.AddHostedServices(subscribeBackgroundService);
                        }
                    }
                
            }
            //  services.AddSingleton(rabbitMQSubscribeDefinitions as IReadOnlyCollection<SubscribeBackgroundService>);
        }
    }

}
