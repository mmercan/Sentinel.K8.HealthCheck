using System;
using Microsoft.Extensions.Logging;
using Sentinel.Models.Scheduler;
using Sentinel.Scheduler.GeneralScheduler.Cron;

namespace Sentinel.Scheduler.GeneralScheduler
{
    public class ScheduledTask<T> where T : IScheduledTask, new()
    {
        private readonly ILogger _logger;

        public ScheduledTask(ILogger logger, T task, DateTime? referenceTime = null)
        {
            if (referenceTime == null) referenceTime = DateTime.UtcNow;

            _logger = logger;
            Uid = task.Uid;
            Schedule = CrontabSchedule.Parse(task.Schedule);
            Task = task;
            NextRunTime = referenceTime.Value;
        }

        public string Uid { get; set; } = default!;
        public CrontabSchedule Schedule { get; set; } = default!;
        public T Task { get; set; } = default!;

        public DateTime LastRunTime { get; set; }
        public DateTime NextRunTime { get; set; }

        public void Increment()
        {
            LastRunTime = NextRunTime;
            NextRunTime = Schedule.GetNextOccurrence(NextRunTime);
        }

        public bool ShouldRun(DateTime currentTime, TimeZoneInfo timeZone)
        {
            var localNextRunTime = TimeZoneInfo.ConvertTime(NextRunTime, timeZone);

            var localCurrentTime = TimeZoneInfo.ConvertTime(currentTime, timeZone);
            var localLastRunTime = TimeZoneInfo.ConvertTime(LastRunTime, timeZone);

            _logger.LogDebug($"ScheduledTask: ShouldRun: localNextRunTime : {localNextRunTime.ToString()} localCurrentTime: {localCurrentTime.ToString()} localLastRunTime: {localLastRunTime.ToString()}");

            return localNextRunTime < localCurrentTime && localLastRunTime != localNextRunTime;
        }

    }
}