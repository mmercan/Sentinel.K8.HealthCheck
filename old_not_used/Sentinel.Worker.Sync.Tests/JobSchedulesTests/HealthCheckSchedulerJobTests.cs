using System;
using System.Threading;
using AutoMapper;
using Moq;
using Quartz;
using Sentinel.K8s;
using Sentinel.K8s.K8sClients;
using Sentinel.Tests.Helpers;
using Sentinel.Worker.Sync.JobSchedules;
using Sentinel.Worker.Sync.TestsHelpers;
using StackExchange.Redis;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Worker.Sync.Tests.JobSchedulesTests
{
    public class HealthCheckSchedulerJobTests
    {
        private ITestOutputHelper _output;
        public HealthCheckSchedulerJobTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void HealthCheckSchedulerJobShouldRun()
        {

            _output.WriteLine("HealthCheckSchedulerJobShouldRun started");


            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<HealthCheckSyncSchedulerJob>();

            var config = new MapperConfiguration(cfg =>
              {
                  cfg.AddProfile(new K8SMapper());
              });


            IConnectionMultiplexer rediscon = RedisExtensions.GetRedisMultiplexer();

            var client = KubernetesClientTestHelper.GetKubernetesClient();
            var mapper = GetIMapperExtension.GetIMapper(cfg => { cfg.AddProfile(new K8SMapper()); });
            var loggerRepo = Sentinel.Tests.Helpers.Helpers.GetLogger<K8sGeneralService>();


            var k8sGeneralService = new K8sGeneralService(client, mapper, loggerRepo);

            HealthCheckSyncSchedulerJob job = new HealthCheckSyncSchedulerJob(
                logger: logger,
                k8sGeneralService,
                mapper: mapper,
                redisMultiplexer: rediscon
            );

            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(3 * 1000);

            var contextMoc = new Mock<IJobExecutionContext>();
            contextMoc.Setup(m => m.CancellationToken).Returns(source.Token);


            var jobtask = job.Execute(contextMoc.Object);
            try { jobtask.Wait(source.Token); }
            catch { }

            // Assert.Throws<OperationCanceledException>(() => jobtask.Wait(source.Token));
        }
    }
}