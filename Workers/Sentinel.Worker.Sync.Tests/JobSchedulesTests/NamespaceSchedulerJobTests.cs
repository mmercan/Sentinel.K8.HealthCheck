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
    public class NamespaceSchedulerJobTests
    {
        private ITestOutputHelper _output;
        public NamespaceSchedulerJobTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void NamespaceSchedulerJobShouldRun()
        {

            _output.WriteLine("NamespaceSchedulerJobShouldRun started");

            var client = KubernetesClientTestHelper.GetKubernetesClient();
            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<NamespaceSchedulerJob>();
            var mapper = GetIMapperExtension.GetIMapper(cfg => { cfg.AddProfile(new K8SMapper()); });
            var rediscon = RedisExtensions.GetRedisMultiplexer();

            NamespaceSchedulerJob job = new NamespaceSchedulerJob(logger, client, mapper, rediscon);

            // CancellationTokenSource source = new CancellationTokenSource();
            // source.CancelAfter(3 * 1000);

            // var contextMoc = new Mock<IJobExecutionContext>();
            // contextMoc.Setup(m => m.CancellationToken).Returns(source.Token);
            // var jobtask = job.Execute(contextMoc.Object);

            // try   { jobtask.Wait(source.Token); }
            // catch { output.WriteLine("NamespaceSchedulerJob Cancelled : ");}

            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(3 * 1000);

            var contextMoc = new Mock<IJobExecutionContext>();
            contextMoc.Setup(m => m.CancellationToken).Returns(source.Token);

            var jobtask = job.Execute(contextMoc.Object);
            // Assert.Throws<OperationCanceledException>(() => jobtask.Wait(source.Token));
            try
            {
                jobtask.Wait(source.Token);
            }
            catch { }

        }
    }
}