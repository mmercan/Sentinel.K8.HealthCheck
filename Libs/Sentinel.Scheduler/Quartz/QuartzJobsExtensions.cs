using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace Sentinel.Scheduler.Quartz
{
    public static class QuartzJobsExtensions
    {
        public static IServiceCollection AddQuartzJobs(this IServiceCollection services, IConfiguration configuration, params Type[] scanMarkers)
        {

            NameValueCollection properties = new NameValueCollection();
            var listJobType = new List<Type>();
            foreach (var marker in scanMarkers.Where(p => p is not null))
            {
                var items = marker.Assembly.ExportedTypes?
                .Where(t => typeof(IJob).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract &&
                    Attribute.IsDefined(t, typeof(QuartzJobAttribute))
                );
                if (items.Any())
                {
                    listJobType.AddRange(items);
                }
            }



            return services.AddQuartz(properties, (quartz) =>
            {
                quartz.SchedulerId = "Blah"; //builder.Environment.ApplicationName;
                quartz.UseMicrosoftDependencyInjectionJobFactory();
                quartz.UseSimpleTypeLoader();
                quartz.UseInMemoryStore();

                quartz.UseDefaultThreadPool(tp => { tp.MaxConcurrency = 10; });

                foreach (Type jobType in listJobType)
                {
                    QuartzJobAttribute? jobAttr = jobType.GetCustomAttributes(typeof(QuartzJobAttribute), true).First() as QuartzJobAttribute;
                    if (jobAttr != null)
                    {
                        string configurationSection = jobAttr.ConfigurationSection;
                        string jobName = string.IsNullOrEmpty(jobAttr.Name) ? jobType.Name : jobAttr.Name;

                        if (configurationSection != null)
                        {
                            //var jobConfig = configuration.GetSection(configurationSection).Get<QuartzJobConfiguration>();
                            //quartz.AddJob(jobType, jobName, jobConfig);
                        }
                        else
                        {
                            quartz
                           .AddJob(jobType, configure: (config) =>
                           {
                               config.WithIdentity(jobName, jobAttr.Group);
                               config.WithDescription(jobAttr.Description);
                               config.StoreDurably(jobAttr.StoreDurably);
                               config.RequestRecovery(jobAttr.RequestRecovery);
                           })
                           .AddTrigger(config =>
                             {
                                 config.ForJob(jobName, jobAttr.Group);
                                 config.WithIdentity(jobName + "Trigger", jobAttr.Group);
                                 config.WithPriority(jobAttr.Priority);
                                 config.WithCronSchedule(jobAttr.CronExpression, builder => builder.InTimeZone(TimeZoneInfo.Local));
                                 config.StartAt(DateBuilder.EvenSecondDate(DateTimeOffset.Now.AddSeconds(jobAttr.DelaySecond)));
                             });


                        }
                    }
                }


            });
        }
    }
}