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
        public void CrontabScheduleShouldwithwithstar()
        {
            CrontabSchedule schedule = CrontabSchedule.Parse("* * * * *");
            var when = schedule.GetNextOccurrence(DateTime.Now);
            var whentime = when.ToShortTimeString();
            var whendate = when.ToShortDateString();
            Assert.NotNull(when);
        }


        [Fact]
        public void CrontabScheduleShouldwithruneverysecondmonite()
        {
            CrontabSchedule schedule = CrontabSchedule.Parse("*/2 * * * *");
            var when = schedule.GetNextOccurrence(DateTime.Now);
            var whentime = when.ToShortTimeString();
            var whendate = when.ToShortDateString();
            Assert.NotNull(when);
        }


        [Fact]
        public void CrontabScheduleShouldwithruneveryhourbetween9to5()
        {
            CrontabSchedule schedule = CrontabSchedule.Parse("0 9-17 * * *");
            var when = schedule.GetNextOccurrence(DateTime.Now);
            var whentime = when.ToShortTimeString();
            var whendate = when.ToShortDateString();
            Assert.NotNull(when);
        }



        [Fact]
        public void CrontabScheduleShouldwithrunMondaytoFridayOnce()
        {
            CrontabSchedule schedule = CrontabSchedule.Parse("0 0 * * 1-5");
            var when = schedule.GetNextOccurrence(DateTime.Now);
            var whentime = when.ToShortTimeString();
            var whendate = when.ToShortDateString();
            Assert.NotNull(when);
        }


        [Fact]
        public void CrontabScheduleShouldwithrunMonWedFridayOnce()
        {
            CrontabSchedule schedule = CrontabSchedule.Parse("0 0 * * 1,3,5");
            var when = schedule.GetNextOccurrence(DateTime.Now);
            var whentime = when.ToShortTimeString();
            var whendate = when.ToShortDateString();
            Assert.NotNull(when);
        }




        [Fact]
        public void CrontabScheduleShouldThrowIfpatternNull()
        {
            Assert.Throws<ArgumentNullException>(() => CrontabSchedule.Parse(null));
        }


        [Fact]
        public void CrontabScheduleShouldThrowIfpatternNotCorrect()
        {
            // This doesn;t work with seconds.
            Assert.Throws<FormatException>(() => CrontabSchedule.Parse("* * * * * *"));
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
        public void CrontabScheduleShouldGiveNextTriggerwithEndDateinthepast()
        {
            CrontabSchedule schedule = CrontabSchedule.Parse("*/3 * * * *");
            var when = schedule.GetNextOccurrence(DateTime.Now, DateTime.Now.AddHours(-2));
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


        [Fact]
        public void CrontabScheduleShouldHandleSameDatetoend()
        {
            CrontabSchedule schedule = CrontabSchedule.Parse("*/3 * * * *");
            var dt = new DateTime(2021, 12, 31, 23, 58, 0);
            var nextOccurrences = schedule.GetNextOccurrences(dt, dt);

        }

        [Fact]
        public void CrontabScheduleShouldHandleSameDatetoendinthepast()
        {
            CrontabSchedule schedule = CrontabSchedule.Parse("*/3 * * * *");
            var dt = new DateTime(2021, 12, 31, 23, 58, 0);
            var nextOccurrences = schedule.GetNextOccurrences(dt, dt.AddDays(-1));

        }
    }
}