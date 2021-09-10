using System.Threading;
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

            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(3 * 1000);

            var contextMoc = new Mock<IJobExecutionContext>();
            contextMoc.Setup(m => m.CancellationToken).Returns(source.Token);


            var jobtask = job.Execute(contextMoc.Object);
            jobtask.Wait(source.Token);
        }
    }
}