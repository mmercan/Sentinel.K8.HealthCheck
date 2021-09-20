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

        }

        public async Task Execute(IJobExecutionContext context)
        {
            // var config = KubernetesClientConfiguration.BuildConfigFromConfigFile();
            // var generic = new GenericClient(config, "sentinel.mercan.io", "v1", "healthchecks");
            // var healthChecks = await generic.ListAsync<HealthCheckResourceList>().ConfigureAwait(false);  //await generic.ReadAsync<HealthCheckResource>("kube0").ConfigureAwait(false);

            // var names = string.Join(',', healthChecks.Items.Select(p => p.Name()).ToArray());

            // _logger.LogWarning(names);
            // return Task.CompletedTask;

            var items = await _k8sclient.ApiClient.ListDeploymentForAllNamespacesAsync();
            var dtoitems = _mapper.Map<IList<DeploymentV1>>(items.Items);
            var syncTime = DateTime.UtcNow;

            foreach (var item in dtoitems)
            {
                item.Name = item.Metadata.Name;
                item.Namespace = item.Metadata.Namespace;
                item.SyncDate = syncTime;

                _logger.LogInformation(item.NameandNamespace + " upsert");

                // await deploymentMongoRepo.Upsert(item, p => p.Name == item.Name && p.Namespace == item.Namespace);
            }
            await _redisDatabase.SetListAsync(items.Items, (item) => { return "Deployment:" + item.Namespace() + ":" + item.Name(); });

            // Type dep = typeof(DeploymentV1);
            // var key = dep.GetProperties().SingleOrDefault(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Count() > 0);
            // var prodname = key?.Name;
            //  var attrs = System.Attribute.GetCustomAttributes(dep);
            //  var attrstring = string.Join(',', attrs.Select(p => p.TypeId.ToString()));
            //  _logger.LogInformation(attrstring);

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