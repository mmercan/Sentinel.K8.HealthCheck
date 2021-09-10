using Moq;
using Quartz;
using Sentinel.Worker.Sync.JobSchedules;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Worker.Sync.Tests.JobSchedulesTests
{
    public class DeploymentSchedulerJobTests
    {
        private ITestOutputHelper output;
        public DeploymentSchedulerJobTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void DeploymentSchedulerJobShouldRun()
        {
            DeploymentSchedulerJob job = new DeploymentSchedulerJob();

            var contextMoc = new Mock<IJobExecutionContext>();

            var jobtask = job.Execute(contextMoc.Object);
            jobtask.Wait();
        }

    }
}