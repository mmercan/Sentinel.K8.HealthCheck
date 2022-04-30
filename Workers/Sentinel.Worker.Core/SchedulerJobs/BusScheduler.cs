using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Quartz;

namespace Scheduler.JobSchedules
{
    [QuartzJob(Name = "BusScheduler", Description = "BusScheduler Check Cron of All Scheduled Repositories and Ad them to RabbitMQ Queues", Group = "Scheduler", CronExpression = "0 */1 * * * ?")]
    public class BusScheduler : IJob
    {
        public BusScheduler()
        {

        }
        public Task Execute(IJobExecutionContext context)
        {
            return Task.CompletedTask;
            //throw new NotImplementedException();
        }
    }
}