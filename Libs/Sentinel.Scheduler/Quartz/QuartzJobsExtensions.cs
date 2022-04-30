using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Quartz;

namespace Sentinel.Scheduler.Quartz
{
    public static class QuartzJobsExtensions
    {
        public static IServiceCollection AddQuartzJobs(this IServiceCollection services, IConfiguration configuration, params Type[] scanMarkers)
        {
            var logger = services.BuildServiceProvider().GetService<ILoggerFactory>()?.CreateLogger("QuartzJobsExtensions");
            NameValueCollection properties = new NameValueCollection();
            var listJobType = new List<Type>();
            foreach (var marker in scanMarkers.Where(p => p is not null))
            {
                var items = marker.Assembly.ExportedTypes?
                .Where(t => typeof(IJob).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract &&
                    Attribute.IsDefined(t, typeof(QuartzJobAttribute))
                );
                if (items != null && items.Any())
                {
                    listJobType.AddRange(items);
                }
            }



            return services.AddQuartz(properties, (quartz) =>
            {
                // quartz.SchedulerId = "Blah"; //builder.Environment.ApplicationName;
                quartz.UseMicrosoftDependencyInjectionJobFactory();
                quartz.UseSimpleTypeLoader();
                quartz.UseInMemoryStore();

                quartz.UseDefaultThreadPool(tp => { tp.MaxConcurrency = 10; });

                foreach (Type jobType in listJobType)
                {

                    QuartzJobAttribute? jobAttr = jobType.GetCustomAttributes(typeof(QuartzJobAttribute), true).First() as QuartzJobAttribute;
                    if (jobAttr != null)
                    {
                        string configurationSectionSelector = jobAttr.ConfigurationSection;
                        string jobName = string.IsNullOrEmpty(jobAttr.Name) ? jobType.Name : jobAttr.Name;
                        string? group = string.IsNullOrEmpty(jobAttr.Group) ? jobType.Assembly.GetName().Name : jobAttr.Group;
                        if (string.IsNullOrEmpty(group)) group = "Default";
                        if (configurationSectionSelector != null)
                        {
                            IConfigurationSection configurationSection = configuration.GetSection(configurationSectionSelector);
                            bool enabled;
                            bool.TryParse(configurationSection["enabled"], out enabled);
                            jobAttr.Enabled = enabled;
                            if (configurationSection["name"] != null)
                            {
                                jobName = configurationSection["name"];
                            }

                            if (configurationSection["group"] != null)
                            {
                                group = jobName = configurationSection["group"];
                            }
                            if (configurationSection["schedule"] != null)
                            {
                                jobAttr.CronExpression = configurationSection["schedule"];
                            }
                            //var jobConfig = configuration.GetSection(configurationSection).Get<QuartzJobConfiguration>();
                            //quartz.AddJob(jobType, jobName, jobConfig);
                        }

                        if (logger != null)
                        {
                            logger.LogInformation("Registering job {jobTypeName} on scheduled {CronExpression} Enabled: {Enabled} ", jobType.Name, jobAttr.CronExpression, jobAttr.Enabled);
                        }
                        if (!jobAttr.Enabled) continue;

                        quartz
                       .AddJob(jobType, configure: (config) =>
                       {
                           config.WithIdentity(jobName, group);
                           config.WithDescription(jobAttr.Description);
                           config.StoreDurably(jobAttr.StoreDurably);
                           config.RequestRecovery(jobAttr.RequestRecovery);
                       })
                       .AddTrigger(config =>
                         {
                             config.ForJob(jobName, group);
                             config.WithIdentity(jobName + "Trigger", group);
                             config.WithPriority(jobAttr.Priority);
                             config.WithCronSchedule(jobAttr.CronExpression, builder => builder.InTimeZone(TimeZoneInfo.Local));
                             config.StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.Now.AddSeconds(jobAttr.DelaySecond)));
                         });
                    }
                }
            });
        }
    }
}