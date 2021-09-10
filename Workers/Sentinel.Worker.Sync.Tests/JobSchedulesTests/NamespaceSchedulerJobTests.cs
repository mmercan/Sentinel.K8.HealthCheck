using Moq;
using Quartz;
using Sentinel.Worker.Sync.JobSchedules;
using Sentinel.Worker.Sync.TestsHelpers;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Worker.Sync.Tests.JobSchedulesTests
{
    public class NamespaceSchedulerJobTests
    {
        private ITestOutputHelper output;
        public NamespaceSchedulerJobTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void NamespaceSchedulerJobShouldRun()
        {

            var client = KubernetesClientTestHelper.GetKubernetesClient();
            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<NamespaceSchedulerJob>();
            NamespaceSchedulerJob job = new NamespaceSchedulerJob(client, logger);

            var contextMoc = new Mock<IJobExecutionContext>();
            var jobtask = job.Execute(contextMoc.Object);
            jobtask.Wait();
        }
    }
}