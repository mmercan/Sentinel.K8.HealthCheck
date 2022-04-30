using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sentinel.Models.Scheduler;
using Sentinel.Scheduler.GeneralScheduler.Cron;

namespace Sentinel.Scheduler.GeneralScheduler
{
    public interface IScheduledTask
    {
        string Uid { get; set; }
        CrontabSchedule Schedule { get; set; }
        DateTime LastRunTime { get; set; }
        DateTime NextRunTime { get; set; }

        IScheduledTaskItem IScheduledTaskItem { get; }

        void Increment();
        bool ShouldRun(DateTime currentTime, TimeZoneInfo timeZone);
    }
}