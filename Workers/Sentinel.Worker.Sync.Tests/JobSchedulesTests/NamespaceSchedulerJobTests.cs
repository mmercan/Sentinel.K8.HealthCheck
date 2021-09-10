using System;
using System.Threading;
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

            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(3 * 1000);

            var contextMoc = new Mock<IJobExecutionContext>();
            contextMoc.Setup(m => m.CancellationToken).Returns(source.Token);

            var jobtask = job.Execute(contextMoc.Object);
            try
            {
                jobtask.Wait(source.Token);
            }
            catch (OperationCanceledException ex)
            {
                output.WriteLine("NamespaceSchedulerJob Cancelled : " + ex.Message);
            }


        }
    }
}