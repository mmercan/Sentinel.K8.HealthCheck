using Moq;
using Quartz;
using Sentinel.Worker.Sync.JobSchedules;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Worker.Sync.Tests.JobSchedulesTests
{
    public class HealthCheckSchedulerJobTests
    {
        private ITestOutputHelper output;
        public HealthCheckSchedulerJobTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void HealthCheckSchedulerJobShouldRun()
        {
            HealthCheckSchedulerJob job = new HealthCheckSchedulerJob();

            var contextMoc = new Mock<IJobExecutionContext>();

            var jobtask = job.Execute(contextMoc.Object);
            jobtask.Wait();
        }
    }
}