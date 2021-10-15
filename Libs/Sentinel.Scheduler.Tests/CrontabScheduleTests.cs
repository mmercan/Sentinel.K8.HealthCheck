using System;
using Sentinel.Scheduler.GeneralScheduler.Cron;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Scheduler.Tests
{
    public class CrontabScheduleTests
    {

        private readonly ITestOutputHelper _output;

        public CrontabScheduleTests(ITestOutputHelper output)
        {
            this._output = output;
        }


        [Fact]
        public void CrontabScheduleShouldGiveNextTrigger()
        {
            CrontabSchedule schedule = CrontabSchedule.Parse("*/3 * * * *");
            var when = schedule.GetNextOccurrence(DateTime.Now);
            var whentime = when.ToShortTimeString();
            var whendate = when.ToShortDateString();
            Assert.NotNull(when);
        }
    }
}