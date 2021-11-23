using System;
using BenchmarkDotNet.Running;

namespace Sentinel.Scheduler.Benchmark
{
    class Program
    {

        static void Main(string[] args)
        {
            BenchmarkRunner.Run<SchedulerRepositoryBenchmark>();
        }

    }
}