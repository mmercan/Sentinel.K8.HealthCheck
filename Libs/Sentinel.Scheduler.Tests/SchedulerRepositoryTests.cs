using Sentinel.Models.K8sDTOs;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Scheduler.Tests
{
    public class SchedulerRepositoryTests
    {
        private readonly ITestOutputHelper _output;

        public SchedulerRepositoryTests(ITestOutputHelper output)
        {
            this._output = output;
        }

        [Fact]
        public void SchedulerRepositoryShouldCreateAnInstance()
        {
            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<SchedulerRepository<HealthCheckResourceV1>>();
            var repo = new SchedulerRepository<HealthCheckResourceV1>(logger);

            var hc = new HealthCheckResourceV1();
            hc.Schedule = "* * * * *";
            hc.Name = "test";
            hc.Namespace = "default";

            repo.Add(hc);

            repo.UpdateSchedulerRepository(hc);

            repo.Remove(hc);


        }

    }
}