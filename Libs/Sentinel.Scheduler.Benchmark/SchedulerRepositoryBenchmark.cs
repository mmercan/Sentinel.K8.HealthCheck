using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using Sentinel.Models.K8sDTOs;

namespace Sentinel.Scheduler.Benchmark
{
    [MemoryDiagnoser]
    [Orderer(SummaryOrderPolicy.FastestToSlowest)]
    [RankColumn]
    public class SchedulerRepositoryBenchmark
    {



        [Benchmark]

        public void GetSchedulerRepository()
        {

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<SchedulerRepository<HealthCheckResourceV1>>();
            var repo = new SchedulerRepository<HealthCheckResourceV1>(logger);

            var hc = new HealthCheckResourceV1();
            hc.Schedule = "* * * * *";
            hc.Name = "test";
            hc.Namespace = "default";

            repo.Add(hc);

            repo.UpdateItem(hc);

            repo.Remove(hc);


        }
    }
}