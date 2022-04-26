using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quartz;

namespace Workers.Sentinel.Worker.Core.Jobs
{
    [QuartzJob(Name = "Job1", Description = "Job1 description", Group = "Job1Group", CronExpression = "0 */1 * * * ?")]
    public class Job1 : IJob
    {
        private readonly IConfiguration _config;

        public Job1(IConfiguration config)
        {
            _config = config;
        }
        public Task Execute(IJobExecutionContext context)
        {
            return Task.CompletedTask;
        }

    }
}