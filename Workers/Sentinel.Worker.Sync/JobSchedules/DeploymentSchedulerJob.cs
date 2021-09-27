using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Quartz;
using Sentinel.K8s;
using Sentinel.Models.CRDs;
using StackExchange.Redis;
using System.Linq;
using System;
using AutoMapper;
using System.Collections.Generic;
using Sentinel.Models.K8sDTOs;
using Sentinel.Worker.Sync.RedisHelpers;


namespace Sentinel.Worker.Sync.JobSchedules
{
    public class DeploymentSchedulerJob : IJob
    {
        private readonly ILogger<DeploymentSchedulerJob> _logger;
        private readonly IKubernetesClient _k8sclient;
        private readonly IMapper _mapper;
        private readonly IDatabase _redisDatabase;

        public DeploymentSchedulerJob(ILogger<DeploymentSchedulerJob> logger, IKubernetesClient k8sclient, IMapper mapper, IConnectionMultiplexer redisMultiplexer)
        {
            _logger = logger;
            _k8sclient = k8sclient;
            _mapper = mapper;
            _redisDatabase = redisMultiplexer.GetDatabase();
            //redisMultiplexer.GetServer().Keys(pattern: "queue:*").ToArray();
            //redisMultiplexer.GetServer().
            var hostandport = redisMultiplexer.GetEndPoints().First();
            var server = redisMultiplexer.GetServer(hostandport);


            var arrs = server.Keys(pattern: "Deployment:*").ToArray();
            var gets = _redisDatabase.StringGet(arrs);
            _logger.LogWarning(arrs.Count().ToString());
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var items = await _k8sclient.ApiClient.ListDeploymentForAllNamespacesAsync();
            var dtoitems = _mapper.Map<IList<DeploymentV1>>(items.Items);
            var syncTime = DateTime.UtcNow;

            foreach (var item in dtoitems)
            {
                item.Name = item.Metadata.Name;
                item.Namespace = item.Metadata.Namespace;
                item.SyncDate = syncTime;

                _logger.LogInformation(item.NameandNamespace + " upsert");
            }
            await _redisDatabase.SetListAsync(items.Items, (item) => { return "Deployment:" + item.Namespace() + ":" + item.Name(); });


            //var fooRedis = new StackExchange.Redis({ keyPrefix: 'Deployment:' });

            // var value = _redisDatabase.ListGetByIndex("Deployment:", -1);
            // _redisDatabase.list
            // var hasvalue = value.HasValue;
            //var value = _redisDatabase.StringGet("Deployment:*");




            // var mongodbservices = await deploymentMongoRepo.GetAllAsync();
            // foreach (var item in mongodbservices)
            // {
            //     if (!dtoitems.Any(p => p.Metadata.Name == item.Name && p.Metadata.Namespace == item.Namespace))
            //     {
            //         item.Deleted = true;
            //         _logger.LogInformation(item.NameandNamespace + " tag as deleted");
            //         await deploymentMongoRepo.UpdateAsync(item);
            //     }
            // }
            // _logger.LogCritical("Deployment Sync Completed ...!");
            //  logger.LogCritical(dtoitems.ToJSON());
        }
    }
}