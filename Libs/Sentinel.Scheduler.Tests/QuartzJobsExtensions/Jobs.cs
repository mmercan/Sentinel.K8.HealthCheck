using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quartz;

namespace Sentinel.Scheduler.Tests.QuartzJobsExtensions
{
    [QuartzJob(Description = "Job1 description", Group = "Job1Group", CronExpression = "0/5 * * * * ?")]
    public class TestJob1 : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return Task.CompletedTask;
        }
    }


    [QuartzJob(ConfigurationSection = "Schedules:NamespaceSyncScheduler")]
    public class TestJob2 : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            return Task.CompletedTask;
        }
    }

}