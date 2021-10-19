using System;
using Sentinel.Models.Scheduler;
using Sentinel.Scheduler.GeneralScheduler.Cron;

namespace Sentinel.Scheduler.GeneralScheduler
{
    public class SchedulerTaskWrapper<T> where T : new()
    {
        public string Uid { get; set; }
        public CrontabSchedule Schedule { get; set; }
        public IScheduledTask Task { get; set; }

        public DateTime LastRunTime { get; set; }
        public DateTime NextRunTime { get; set; }

        public T Item { get; set; }

        public void Increment()
        {
            LastRunTime = NextRunTime;
            NextRunTime = Schedule.GetNextOccurrence(NextRunTime);

            string nxt = NextRunTime.ToString();
        }

        public bool ShouldRun(DateTime currentTime)
        {
            return NextRunTime < currentTime && LastRunTime != NextRunTime;
        }
    }
}