using Sentinel.Models.K8sDTOs;
using Sentinel.Scheduler.GeneralScheduler;
using Sentinel.Scheduler.GeneralScheduler.Cron;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Scheduler.Tests
{
    public class SchedulerTaskWrapperTests
    {

        private readonly ITestOutputHelper _output;

        public SchedulerTaskWrapperTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Fact]
        public void SchedulerTaskWrapperShouldCreateaNewInstance()
        {
            SchedulerTaskWrapper<HealthCheckResourceV1> wrapper = new GeneralScheduler.SchedulerTaskWrapper<HealthCheckResourceV1>();
            wrapper.Schedule = CrontabSchedule.Parse("*/3 * * * *");
            wrapper.Increment();
            wrapper.ShouldRun(System.DateTime.Now);
            //CrontabSchedule schedule 
            //    var dt = new DateTime(2021, 12, 31, 23, 58, 0);
            //    var nextOccurrences = schedule.GetNextOccurrences(dt, dt.AddDays(-1));
        }

    }
}