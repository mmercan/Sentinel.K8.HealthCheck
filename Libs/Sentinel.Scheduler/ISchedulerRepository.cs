using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sentinel.Models.Scheduler;
using Sentinel.Scheduler.GeneralScheduler;

namespace Sentinel.Scheduler
{
    public interface ISchedulerRepository
    {
        List<IScheduledTask> ScheduledTaskCopy { get; }
        // object ScheduledTasks { get; }

        //List<IScheduledTask> ScheduledTasks { get; }
        //List<ScheduledTask<IScheduledTask>>
    }
}