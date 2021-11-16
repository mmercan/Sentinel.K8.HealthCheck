using System;
using System.Threading;
using Moq;
using Quartz;
using Sentinel.K8s;
using Sentinel.Tests.Helpers;
using Sentinel.Worker.Sync.JobSchedules;
using Sentinel.Worker.Sync.TestsHelpers;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Worker.Sync.Tests.JobSchedulesTests
{
    public class ServiceSchedulerJobTests
    {
        private ITestOutputHelper _output;
        public ServiceSchedulerJobTests(ITestOutputHelper output)
        {
            _output = output;
        }


        [Fact]
        public void ServiceSchedulerJobShouldRun()
        {

            _output.WriteLine("ServiceSchedulerJobShouldRun started");

            var client = KubernetesClientTestHelper.GetKubernetesClient();
            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<ServiceSchedulerJob>();
            var mapper = GetIMapperExtension.GetIMapper(cfg => { cfg.AddProfile(new K8SMapper()); });
            var rediscon = RedisExtensions.GetRedisMultiplexer();

            ServiceSchedulerJob job = new ServiceSchedulerJob(logger, client, mapper, rediscon);

            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(20 * 1000);

            var contextMoc = new Mock<IJobExecutionContext>();
            contextMoc.Setup(m => m.CancellationToken).Returns(source.Token);

            var jobtask = job.Execute(contextMoc.Object);
            //Assert.Throws<OperationCanceledException>(() => jobtask.Wait(source.Token));
            try
            {
                jobtask.Wait(source.Token);
            }
            catch { }
        }
    }
}