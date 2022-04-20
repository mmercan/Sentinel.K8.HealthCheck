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
using Sentinel.K8s.K8sClients;

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


            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<ServiceSyncSchedulerJob>();
            var rediscon = RedisExtensions.GetRedisMultiplexer();

            var client = KubernetesClientTestHelper.GetKubernetesClient();
            var mapper = GetIMapperExtension.GetIMapper(cfg => { cfg.AddProfile(new K8SMapper()); });
            var loggerRepo = Sentinel.Tests.Helpers.Helpers.GetLogger<K8sGeneralService>();

            var k8sGeneralService = new K8sGeneralService(client, mapper, loggerRepo);

            ServiceSyncSchedulerJob job = new ServiceSyncSchedulerJob(logger, k8sGeneralService, rediscon);

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