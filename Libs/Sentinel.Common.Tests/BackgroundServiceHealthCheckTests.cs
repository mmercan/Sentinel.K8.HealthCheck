using Microsoft.Extensions.Diagnostics.HealthChecks;
using Moq;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Common.Tests
{
    public class BackgroundServiceHealthCheckTests
    {
        private readonly ITestOutputHelper _output;

        public BackgroundServiceHealthCheckTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public void BackgroundServiceHealthCheckShouldInitialize()
        {
            // var contextMoc = new Mock<HealthCheckContext>();
            HealthCheckContext contextMoc = new HealthCheckContext();

            //contextMoc.Setup(m => m.CancellationToken).Returns(source.Token);


            BackgroundServiceHealthCheck check = new BackgroundServiceHealthCheck();


            var check_1 = check.CheckHealthAsync(contextMoc);
            Assert.Equal(HealthStatus.Degraded, check_1.Result.Status);

            check.ReportHealthy();
            var check_2 = check.CheckHealthAsync(contextMoc);
            Assert.Equal(HealthStatus.Healthy, check_2.Result.Status);

            check.ReportUnhealthy();
            var check_3 = check.CheckHealthAsync(contextMoc);
            Assert.Equal(HealthStatus.Unhealthy, check_3.Result.Status);
        }


        [Fact]
        public void BackgroundServiceHealthCheckShouldDegradedonNewObjects()
        {

            HealthCheckContext contextMoc = new HealthCheckContext();
            BackgroundServiceHealthCheck check = new BackgroundServiceHealthCheck();


            var check_1 = check.CheckHealthAsync(contextMoc);
            Assert.Equal(HealthStatus.Degraded, check_1.Result.Status);
        }


        [Fact]
        public void BackgroundServiceHealthCheckShouldChangeStates()
        {
            HealthCheckContext contextMoc = new HealthCheckContext();
            BackgroundServiceHealthCheck check = new BackgroundServiceHealthCheck();


            var check_1 = check.CheckHealthAsync(contextMoc);
            Assert.Equal(HealthStatus.Degraded, check_1.Result.Status);

            check.ReportHealthy();
            var check_2 = check.CheckHealthAsync(contextMoc);
            Assert.Equal(HealthStatus.Healthy, check_2.Result.Status);

            check.ReportUnhealthy();
            var check_3 = check.CheckHealthAsync(contextMoc);
            Assert.Equal(HealthStatus.Unhealthy, check_3.Result.Status);
        }
    }
}