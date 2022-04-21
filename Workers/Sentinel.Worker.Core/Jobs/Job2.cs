using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quartz;

namespace Workers.Sentinel.Worker.Core.Jobs
{
    [QuartzJob(ConfigurationSection = "Schedules:NamespaceSyncScheduler")]
    public class Job2 : IJob
    {
        private readonly IConfiguration _config;

        public Job2(IConfiguration config)
        {
            _config = config;
        }
        public Task Execute(IJobExecutionContext context)
        {
            return Task.CompletedTask;
        }

    }
}