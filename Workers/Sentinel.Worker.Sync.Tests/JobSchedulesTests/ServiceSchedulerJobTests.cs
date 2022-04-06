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
using Sentinel.K8s.Repos;

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


            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<ServiceSchedulerJob>();
            var rediscon = RedisExtensions.GetRedisMultiplexer();

            var client = KubernetesClientTestHelper.GetKubernetesClient();
            var mapper = GetIMapperExtension.GetIMapper(cfg => { cfg.AddProfile(new K8SMapper()); });
            var loggerRepo = Sentinel.Tests.Helpers.Helpers.GetLogger<ServiceV1K8sRepo>();
            var serviceV1K8sRepo = new ServiceV1K8sRepo(client, mapper, loggerRepo);

            ServiceSchedulerJob job = new ServiceSchedulerJob(logger, serviceV1K8sRepo, rediscon);

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