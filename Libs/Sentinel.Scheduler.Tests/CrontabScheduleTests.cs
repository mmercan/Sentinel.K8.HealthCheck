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


        [Fact]
        public void CrontabScheduleShouldGiveNextTriggerwithEndDate()
        {
            CrontabSchedule schedule = CrontabSchedule.Parse("*/3 * * * *");
            var when = schedule.GetNextOccurrence(DateTime.Now, DateTime.Now.AddHours(2));
            var whentime = when.ToShortTimeString();
            var whendate = when.ToShortDateString();
            Assert.NotNull(when);
        }



        [Fact]
        public void CrontabScheduleShouldGiveToString()
        {
            CrontabSchedule schedule = CrontabSchedule.Parse("*/3 * * * *");
            var when = schedule.GetNextOccurrence(DateTime.Now, DateTime.Now.AddHours(2));
            var whentime = when.ToShortTimeString();
            var whendate = when.ToShortDateString();

            schedule.ToString();

            Assert.NotNull(when);
        }



        [Fact]
        public void CrontabScheduleShouldGetNextOccurrences()
        {
            CrontabSchedule schedule = CrontabSchedule.Parse("*/3 * * * *");
            var when = schedule.GetNextOccurrence(DateTime.Now, DateTime.Now.AddHours(2));
            var whentime = when.ToShortTimeString();
            var whendate = when.ToShortDateString();

            var nextOccurrences = schedule.GetNextOccurrences(DateTime.Now, DateTime.Now.AddHours(3));
            Assert.NotNull(when);
        }



        [Fact]
        public void CrontabScheduleShouldHandleLastDayofYear()
        {
            CrontabSchedule schedule = CrontabSchedule.Parse("*/3 * * * *");
            var dt = new DateTime(2021, 12, 31, 23, 58, 0);
            var when = schedule.GetNextOccurrence(dt, dt.AddHours(2));
            var whentime = when.ToShortTimeString();
            var whendate = when.ToShortDateString();

            var nextOccurrences = schedule.GetNextOccurrences(dt, dt.AddHours(3));
            Assert.NotNull(when);
        }
    }
}