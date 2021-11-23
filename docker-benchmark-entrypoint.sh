#!/bin/bash

# dotnet build ./Sentinel.K8.HealthCheck.sln

dotnet build ./Libs/Sentinel.Scheduler.Benchmark -c Release --output /output/Sentinel.Scheduler.Benchmark
echo "Console output"

dotnet /output/Sentinel.Scheduler.Benchmark/Sentinel.Scheduler.Benchmark.dll --filter *


mkdir -p /Benchmarks/Sentinel.Scheduler
cp -r ./BenchmarkDotNet.Artifacts/ /Benchmarks/Sentinel.Scheduler
ls /Benchmarks -R 