using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Matchers;
using Sentinel.Scheduler.Quartz;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Scheduler.Tests.QuartzJobsExtensions
{
    public class QuartzJobsExtensionsTests
    {
        private readonly ITestOutputHelper _output;
        private readonly IServiceCollection serviceCollection;
        private readonly IConfiguration config;
        public QuartzJobsExtensionsTests(ITestOutputHelper output)
        {
            this._output = output;

            serviceCollection = new ServiceCollection();
            var provider = serviceCollection.BuildServiceProvider();



            var myConfiguration = new Dictionary<string, string>
            {
                {"Rediskey:HealthChecks", "HealthChecks"},
                { "Rediskey:Services", "Services"},
                {"Rediskey:HealCheckServiceNotFound", "HealCheckServiceNotFound"},
                {"queue:healthcheck", "healthcheck"},
                {"Schedules:NamespaceSyncScheduler:schedule","0 */1 * * * ?"},
                {"Schedules:NamespaceSyncScheduler:enabled","true"},
            };

            config = new ConfigurationBuilder()
             .AddInMemoryCollection(myConfiguration)
             .Build();

            serviceCollection.AddQuartzJobs(config, typeof(TestJob1));
            serviceCollection.AddLogging();
        }

        [Fact]
        public async Task AddQuartzJobsShouldAddQuartz()
        {

            var provider = serviceCollection.BuildServiceProvider();
            var schedulerFactory = provider.GetService<ISchedulerFactory>();
            if (schedulerFactory == null) return;

            var schedulers = await schedulerFactory.GetScheduler();
            var groupNames = await schedulers.GetJobGroupNames();
            foreach (var group in groupNames)
            {
                var groupMatcherJob = GroupMatcher<JobKey>.GroupContains(group);
                var groupMatcherTrigger = GroupMatcher<TriggerKey>.GroupContains(group);
                var jobkeys = await schedulers.GetJobKeys(groupMatcherJob);
                var triggerKeys = await schedulers.GetTriggerKeys(groupMatcherTrigger);
                foreach (var jobkey in jobkeys)
                {
                    var jobDetail = await schedulers.GetJobDetail(jobkey);
                    if (jobDetail == null) return;
                    var name = jobDetail.Key.Name;

                    var trigger = await schedulers.GetTrigger(new TriggerKey(name + "Trigger", jobDetail.Key.Group));
                    if (trigger == null) return;
                    var time = trigger.GetNextFireTimeUtc();
                    var timestr = time.Value.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss");
                }
            }
            var jobs = schedulers.Context;
            // Assert.NotNull(job1);
        }
    }
}