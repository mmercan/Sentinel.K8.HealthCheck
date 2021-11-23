#!/bin/bash

dotnet build ./Sentinel.K8.HealthCheck.sln

dotnet build ./Libs/Sentinel.Scheduler.Benchmark -c Release --output ./output/Sentinel.Scheduler.Benchmark
echo "Console output"

dotnet ./output/Sentinel.Scheduler.Benchmark/Sentinel.Scheduler.Benchmark.dll
