## Run Test with details and coverages

```
dotnet watch test --logger:"console;verbosity=detailed" /p:CollectCoverage=true

```

## Workers

### General
- Sync (c) :gear:
- Scheduler (c)
- Comms (Mail, Teams, Signalr etc..) (c)

### Runners
- HealthCheck (c)
- Replica Scaler (c)
- Screen Shot (c)
- Keyvault to Secret puller (c)


## Apis
- Api.HealthMonitoring (c)
 
## Libs
- Sentinel.Common :blue_book:
- Sentinel.K8s :blue_book:
- Sentinel.Models :blue_book:
