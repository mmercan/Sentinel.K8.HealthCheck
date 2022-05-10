using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentinel.Common.Extensions;
using Sentinel.PubSub.BackgroundServices;

namespace Sentinel.PubSub.Middlewares
{
    public static class RabbitMQSubscribeDefinitionMiddleware
    {
        public static void AddRabbitMQSubscribeDefinitions(this IServiceCollection services, IConfiguration configuration, params Type[] scanMarkers)
        {

            var rabbitMQSubscribeDefinitions = new List<Type>();
            foreach (var marker in scanMarkers)
            {
                // var items = marker.Assembly.ExportedTypes.Where(
                //     t => typeof(SubscribeBackgroundService).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract
                //     && Attribute.IsDefined(t, typeof(RabbitMQSubscribeAttribute))
                // );

                var items2 = marker.Assembly.ExportedTypes.Where(
                    t => t.BaseType != null && t.BaseType.IsGenericType == true
                    && t.BaseType.GetGenericTypeDefinition() == typeof(SubscribeBackgroundService<>)
                    && !t.IsInterface && !t.IsAbstract
                    && Attribute.IsDefined(t, typeof(RabbitMQSubscribeAttribute))
                );

                // if (items != null && items.Any())
                // {
                //     rabbitMQSubscribeDefinitions.AddRange(items);
                // }
                if (items2 != null && items2.Any())
                {
                    rabbitMQSubscribeDefinitions.AddRange(items2);
                }
            }
            foreach (var subscribeBackgroundService in rabbitMQSubscribeDefinitions)
            {
                RabbitMQSubscribeAttribute? rabbitAttr = subscribeBackgroundService.GetCustomAttributes(typeof(RabbitMQSubscribeAttribute), true).First() as RabbitMQSubscribeAttribute;
                if (rabbitAttr != null)
                {
                    bool enabled = rabbitAttr.Enabled;
                    if (enabled)
                    {
                        services.AddHostedServices(subscribeBackgroundService);
                    }
                }

            }
            //  services.AddSingleton(rabbitMQSubscribeDefinitions as IReadOnlyCollection<SubscribeBackgroundService>);
        }
    }

}
