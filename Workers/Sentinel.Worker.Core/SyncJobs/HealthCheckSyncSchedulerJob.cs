using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Libs.Sentinel.K8s;
using Polly;
using Polly.Retry;
using Quartz;
using Sentinel.K8s.K8sClients;
using Sentinel.Models.CRDs;
using Sentinel.Models.K8sDTOs;
using Sentinel.Models.Redis;
using Sentinel.Redis;
using Sentinel.Scheduler.Helpers;
using StackExchange.Redis;

namespace Workers.Sentinel.Worker.Core.SyncJobs
{
    [QuartzJob(ConfigurationSection = "Schedules:HealthCheckSyncScheduler", DelaySecond = 30)]
    public class HealthCheckSyncSchedulerJob : IJob
    {
        private readonly ILogger<HealthCheckSyncSchedulerJob> _logger;
        private readonly K8sGeneralService _k8sGeneralService;
        private readonly IMapper _mapper;
        private readonly K8MemoryRepository _k8MemoryRepository;
        private readonly IRedisDictionary<HealthCheckResourceV1> redisDic;
        private readonly RedisDictionary<ServiceV1> redisServiceDictionary;
        private readonly RedisDictionary<HealthCheckResourceV1> redisHealCheckServiceNotFoundDictionary;
        protected readonly RetryPolicy policy;
        private readonly object timezone;

        public HealthCheckSyncSchedulerJob(ILogger<HealthCheckSyncSchedulerJob> logger, IConfiguration configuration,
        K8MemoryRepository k8MemoryRepository, K8sGeneralService k8sGeneralService, IMapper mapper, IConnectionMultiplexer redisMultiplexer)
        {
            _logger = logger;
            _k8sGeneralService = k8sGeneralService;
            _mapper = mapper;
            _k8MemoryRepository = k8MemoryRepository;
            redisDic = new RedisDictionary<HealthCheckResourceV1>(redisMultiplexer, _logger, "HealthChecks");
            redisServiceDictionary = new RedisDictionary<ServiceV1>(redisMultiplexer, _logger, configuration["Rediskey:Services:RedisKey"]);
            redisHealCheckServiceNotFoundDictionary = new RedisDictionary<HealthCheckResourceV1>(redisMultiplexer, _logger, configuration["Rediskey:HealCheckServiceNotFound:RedisKey"]);

            if (!string.IsNullOrWhiteSpace(configuration["timezone"]))
            {
                timezone = configuration["timezone"];
            }
            else
            {
                timezone = "Australia/Melbourne";
            }

            policy = Policy.Handle<RedisTimeoutException>().Or<RedisConnectionException>().WaitAndRetry(new[]
              {
                TimeSpan.FromSeconds(1),
                TimeSpan.FromSeconds(2),
                TimeSpan.FromSeconds(3)
              }, (ex, timeSpan, retryCount, context) =>
              {
                  _logger.LogError(ex, "BusScheduler : Polly retry {retryCount}  Error Finding Service Related to HealthCheckResourceV1", retryCount.ToString());
                  //   if (retryCount == 2) {       throw ex;   }
                  // Add logic to be executed before each retry, such as logging    
              });

        }
        public async Task Execute(IJobExecutionContext context)
        {
            var checks = await _k8sGeneralService.HealthCheckResourceClient.GetAllHealthCheckResourcesAsync();
            checks.ForEach(async check =>
            {
                if (string.IsNullOrEmpty(check.Status?.Phase))
                {
                    await _k8sGeneralService.HealthCheckResourceClient.UpdateStartusAsync(check, HealthCheckResource.HealthCheckResourceStatusPhase.AddedtoRedis);
                }
            });
            var dtoitems = _mapper.Map<IList<HealthCheckResourceV1>>(checks);

            var syncTime = DateTime.UtcNow;
            dtoitems.ForEach(p =>
            {
                p.LatestSyncDateUTC = syncTime;
                var service = p.FindServiceRelatedtoHealthCheckResourceV1(_k8MemoryRepository.ServicesDic);
                p.RelatedService = service;
                if (service == null)
                {
                    _logger.LogCritical("BusScheduler : Error Finding Service Related to HealthCheckResourceV1 Logged in RedisHealCheckServiceNotFoundDictionary Key: {key}", p.Name);
                    policy.Execute(() =>
                    {
                        redisHealCheckServiceNotFoundDictionary.Add(p);
                    });
                }
            });

            _k8MemoryRepository.HealthChecks = dtoitems;
            redisDic.Sync(dtoitems);

            _logger.LogInformation("{count} HealthChecks have been synced Which {service} has RelatedServices",
                checks.Count.ToString(), dtoitems.Count(p => p.RelatedService != null).ToString());
        }
    }
}