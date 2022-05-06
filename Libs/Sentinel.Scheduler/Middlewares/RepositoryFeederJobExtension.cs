using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Libs.Sentinel.Scheduler;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sentinel.Models.Scheduler;

namespace Sentinel.Scheduler.Middlewares
{
    public static class RepositoryFeederMiddleware
    {


        public static void AddSchedulerRedisRepositoryFeeder<T>(this IServiceCollection services, IConfigurationSection Section) where T : IScheduledTaskItem
        {
            services.Configure<RedisKeyFeederOption<T>>(Section);
            services.AddSingleton<SchedulerRepository<T>>();
            services.AddSingleton<SchedulerRedisRepositoryFeeder<T>>();
        }


        public static void AddSchedulerRedisRepositoryFeeder<T>(this IServiceCollection services, string RedisKey) where T : IScheduledTaskItem
        {
            services.Configure<RedisKeyFeederOption<T>>(opt => { opt.RedisKey = RedisKey; });
            services.AddSingleton<SchedulerRepository<T>>();
            services.AddSingleton<SchedulerRedisRepositoryFeeder<T>>();
        }

        // public static void AddRepositoryFeeders(this IServiceCollection services, IConfiguration configuration, params Type[] scanMarkers)
        // {
        //     var schedulerRepositoryFeeders = new List<Type>();
        //     foreach (var marker in scanMarkers)
        //     {
        //         var items = marker.Assembly.ExportedTypes.Where(
        //             t => typeof(ISchedulerRepositoryFeeder).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract
        //         );
        //         if (items != null && items.Any())
        //         {
        //             schedulerRepositoryFeeders.AddRange(items);
        //         }
        //     }
        //     foreach (var repofeeder in schedulerRepositoryFeeders)
        //     {
        //         // RabbitMQSubscribeAttribute? rabbitAttr = subscribeBackgroundService.GetCustomAttributes(typeof(RabbitMQSubscribeAttribute), true).First() as RabbitMQSubscribeAttribute;
        //         // if (rabbitAttr != null)
        //         // {
        //         //     bool enabled = rabbitAttr.Enabled;
        //         //     if (enabled)
        //         //     {
        //         if (repofeeder.IsGenericType && repofeeder.GetGenericTypeDefinition() == typeof(SchedulerRedisRepositoryFeeder<>))
        //         {
        //             var genericType = repofeeder.GenericTypeArguments[0];
        //         }
        //         services.AddSingleton(repofeeder);
        //         //    }
        //         // }
        //     }
        //     //  services.AddSingleton(rabbitMQSubscribeDefinitions as IReadOnlyCollection<SubscribeBackgroundService>);
        // }
    }
}