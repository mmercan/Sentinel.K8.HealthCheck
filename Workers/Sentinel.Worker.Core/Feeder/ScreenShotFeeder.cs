using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Libs.Sentinel.Scheduler;
using Microsoft.Extensions.Options;
using Sentinel.Models.K8sDTOs;
using Sentinel.Scheduler;
using StackExchange.Redis;

namespace Sentinel.Worker.Core.Feeder
{
    public class ScreenShotFeeder : SchedulerRedisRepositoryFeeder<DeploymentScalerResourceV1>
    {
        public ScreenShotFeeder(SchedulerRepository<DeploymentScalerResourceV1> schedulerRepository,
        ILogger<SchedulerRedisRepositoryFeeder<DeploymentScalerResourceV1>> logger,
        IConnectionMultiplexer multiplexer,
        IOptions<RedisKeyFeederOption<DeploymentScalerResourceV1>> redisKeyFeederOption) :
        base(schedulerRepository, logger, multiplexer, redisKeyFeederOption)
        {
        }
    }
}