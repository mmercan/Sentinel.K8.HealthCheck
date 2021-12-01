## Run Test with details and coverages


## Current project
```
dotnet watch test --logger:"console;verbosity=detailed" /p:CollectCoverage=true

```

## Solution
```
dotnet watch -p .\Sentinel.K8.HealthCheck.sln test --logger:"console;verbosity=detailed" /p:CollectCoverage=true
```
## Pipelines
### test :
[![Sentinel Test](https://github.com/mmercan/Sentinel.K8.HealthCheck/actions/workflows/docker-image.yml/badge.svg)](https://github.com/mmercan/Sentinel.K8.HealthCheck/actions/workflows/docker-image.yml)
### Quality Gate:
[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=Sentinel.Health.k8&metric=alert_status)](https://sonarcloud.io/dashboard?id=Sentinel.Health.k8)

## Workers

### General
- Sync :gear:
- Scheduler :gear:
- Comms (Mail, Teams, Signalr etc..) :gear:

### Runners
- HealthCheck :gear:
- Replica Scaler :gear:
- Screen Shot :gear:
- Keyvault to Secret puller :gear:


## Apis
- Api.HealthMonitoring :cloud:
 
## Libs
- Sentinel.Common :blue_book:
- Sentinel.K8s :blue_book:
- Sentinel.Models :blue_book:



  :gear: Workers
  :blue_book: Class Library
  :cloud: Api Web Apps


# Port-forward
  kubectl port-forward service/sentinel-worker-scheduler-dev 8080:80 -n sentinel-healthcheck

  sentinel-worker-healthchecker-dev
  sentinel-worker-scheduler-dev
  sentinel-worker-sync-dev

  sentinel-worker-secretgenerator-dev
  sentinel-worker-screenshot-dev
  sentinel-worker-scaler-dev
  sentinel-worker-comms-dev
  sentinel-api-healthmonitoring-dev
  sentinel-dashboard-dev

  healthcheck-rabbitmq-dev-http

  healthcheck-rabbitmq-dev

  healthcheck-redis-dev


# LogLevel 
  // https://docs.microsoft.com/en-us/dotnet/api/microsoft.extensions.logging.loglevel?view=dotnet-plat-ext-5.0

-  Trace
-  Debug
-  Critical
-  Information
-  Warning
-  Error
-  Critical
-  None



# Compose Run
```
docker-compose -f dockercompose-sonar.yml up --build sentinel-healthcheck-test
```