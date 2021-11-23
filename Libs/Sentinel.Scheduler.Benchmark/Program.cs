using System;
using BenchmarkDotNet.Running;

namespace Sentinel.Scheduler.Benchmark
{
    class Program
    {

        static void Main(string[] args)
        {
            BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);

            BenchmarkRunner.Run<SchedulerRepositoryBenchmark>();
            BenchmarkRunner.Run<SchedulerRepositoryBenchmarkSec>();
        }

    }
}