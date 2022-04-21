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

namespace Libs.Sentinel.Scheduler.Tests.QuartzJobsExtensions
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
                {"queue:healthcheck", "healthcheck"}

            };

            config = new ConfigurationBuilder()
             .AddInMemoryCollection(myConfiguration)
             .Build();

            serviceCollection.AddQuartzJobs(config, typeof(Job1));
            serviceCollection.AddLogging();
        }

        [Fact]
        public async Task AddQuartzJobsShouldAddQuartz()
        {

            var provider = serviceCollection.BuildServiceProvider();
            var job1 = provider.GetService<ISchedulerFactory>();



            var schedulers = await job1.GetScheduler();
            var groupNames = await schedulers.GetJobGroupNames();
            foreach (var group in groupNames)
            {
                var groupMatcher = GroupMatcher<JobKey>.GroupContains(group);
                var jobkeys = await schedulers.GetJobKeys(groupMatcher);
                foreach (var jobkey in jobkeys)
                {
                    var jobDetail = await schedulers.GetJobDetail(jobkey);
                    var name = jobDetail.Key.Name;
                }
            }
            var jobs = schedulers.Context;
            Assert.NotNull(job1);
        }
    }
}