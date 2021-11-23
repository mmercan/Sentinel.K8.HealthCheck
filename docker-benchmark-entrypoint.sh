#!/bin/bash

# dotnet build ./Sentinel.K8.HealthCheck.sln

dotnet build ./Libs/Sentinel.Scheduler.Benchmark -c Release --output /output/Sentinel.Scheduler.Benchmark
echo "Console output"

dotnet /output/Sentinel.Scheduler.Benchmark/Sentinel.Scheduler.Benchmark.dll --artifacts /Benchmarks/Sentinel.Scheduler --filter *


mkdir -p /Benchmarks/Sentinel.Scheduler
cp -r ./BenchmarkDotNet.Artifacts/results /Benchmarks/Sentinel.Scheduler