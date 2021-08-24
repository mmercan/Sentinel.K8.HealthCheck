## Run Test with details and coverages

```
dotnet watch test --logger:"console;verbosity=detailed" /p:CollectCoverage=true

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