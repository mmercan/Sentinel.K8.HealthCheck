using System;
using System.Threading;
using AutoMapper;
using Moq;
using Quartz;
using Sentinel.K8s;
using Sentinel.Tests.Helpers;
using Sentinel.Worker.Sync.JobSchedules;
using Sentinel.Worker.Sync.TestsHelpers;
using StackExchange.Redis;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Worker.Sync.Tests.JobSchedulesTests
{
    public class DeploymentScalerSchedulerJobTests
    {
        private ITestOutputHelper _output;
        public DeploymentScalerSchedulerJobTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void DeploymentScalerSchedulerJobShouldRun()
        {
            _output.WriteLine("DeploymentScalerSchedulerJobShouldRun Started");

            var client = KubernetesClientTestHelper.GetKubernetesClient();
            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<DeploymentScalersShedulerJob>();

            var config = new MapperConfiguration(cfg =>
              {
                  cfg.AddProfile(new K8SMapper());
              });
            var mapper = config.CreateMapper();

            IConnectionMultiplexer rediscon = RedisExtensions.GetRedisMultiplexer();



            DeploymentScalersShedulerJob job = new DeploymentScalersShedulerJob(
                logger: logger,
                k8sclient: client,
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