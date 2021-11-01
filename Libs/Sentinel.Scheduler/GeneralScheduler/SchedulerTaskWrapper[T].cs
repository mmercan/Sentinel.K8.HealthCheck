using System;
using Microsoft.Extensions.Logging;
using Sentinel.Models.Scheduler;
using Sentinel.Scheduler.GeneralScheduler.Cron;

namespace Sentinel.Scheduler.GeneralScheduler
{
    public class SchedulerTaskWrapper<T> where T : new()
    {
        private readonly ILogger _logger;

        public SchedulerTaskWrapper(ILogger logger)
        {
            _logger = logger;
        }

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

        public bool ShouldRun(DateTime currentTime, TimeZoneInfo timeZone)
        {
            var localNextRunTime = TimeZoneInfo.ConvertTime(NextRunTime, timeZone);

            var localCurrentTime = TimeZoneInfo.ConvertTime(currentTime, timeZone);
            var localLastRunTime = TimeZoneInfo.ConvertTime(LastRunTime, timeZone);

            _logger.LogInformation($"SchedulerTaskWrapper: ShouldRun: localNextRunTime : {localNextRunTime.ToString()} localCurrentTime: {localCurrentTime.ToString()} localLastRunTime: {localLastRunTime.ToString()}");

            return localNextRunTime < localCurrentTime && localLastRunTime != localNextRunTime;
        }
    }
}