using System;
using Microsoft.Extensions.Configuration;
using Quartz;

namespace Sentinel.Worker.Sync
{
    public static class QuartzConfiguratorExtension
    {
        public static IServiceCollectionQuartzConfigurator AddSchedulerJob<T>(
            this IServiceCollectionQuartzConfigurator configurator,
            IConfigurationSection configurationSection, int delaySecond = 10
        ) where T : IJob
        {
            if (configurationSection["enabled"] != null
            && configurationSection["enabled"] == "true")
            {

                var name = typeof(T).Name;

                configurator.ScheduleJob<T>(trigger => trigger
                .WithIdentity(name)
                .StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.UtcNow.AddSeconds(delaySecond)))
                .WithCronSchedule(configurationSection["schedule"])
                .WithDescription(name + " trigger configured run in Cron"));
            }

            return configurator;
        }
    }
}