using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Quartz;
using Sentinel.K8s;
using Sentinel.Models.K8sDTOs;
using Sentinel.Redis;
using StackExchange.Redis;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace Sentinel.Worker.Sync.JobSchedules
{
    public class ServiceSchedulerJob : IJob
    {
        private readonly IKubernetesClient _k8sclient;
        private readonly ILogger<ServiceSchedulerJob> _logger;
        private readonly IMapper _mapper;
        private readonly RedisDictionary<ServiceV1> redisDicServices;
        private readonly RedisDictionary<ServiceV1> redisDicIngresses;
        private readonly RedisDictionary<ServiceV1> redisDicVirtualServices;

        public ServiceSchedulerJob(ILogger<ServiceSchedulerJob> logger, IKubernetesClient k8sclient, IMapper mapper, IConnectionMultiplexer redisMultiplexer)
        {
            _k8sclient = k8sclient;
            _logger = logger;
            _mapper = mapper;
            redisDicServices = new RedisDictionary<ServiceV1>(redisMultiplexer, _logger, "Services");
            redisDicIngresses = new RedisDictionary<ServiceV1>(redisMultiplexer, _logger, "Ingresses");
            redisDicVirtualServices = new RedisDictionary<ServiceV1>(redisMultiplexer, _logger, "VirtualServices");
        }

        public Task Execute(IJobExecutionContext context)
        {
            List<VirtualServiceV1> virtualservices = new List<VirtualServiceV1>();

            var servicesTask = _k8sclient.ApiClient.ListServiceForAllNamespacesWithHttpMessagesAsync();
            var ingressesTask = _k8sclient.ApiClient.ListIngressForAllNamespacesWithHttpMessagesAsync();
            var virtualservicesTask = _k8sclient.ListClusterCustomObjectAsync("networking.istio.io", "v1alpha3", "virtualservices");

            Task.WaitAll(servicesTask, ingressesTask, virtualservicesTask);

            var ingresses = ingressesTask.Result.Body.Items;
            var virtualservicesJson = virtualservicesTask.Result;
            var services = servicesTask.Result;

            foreach (var item in virtualservicesJson)
            {
                var virtualService = VirtualServiceV1.ConvertFromJTokenToVirtualServiceV1(item);
                virtualservices.Add(virtualService);
            }

            var dtoitems = _mapper.Map<IList<ServiceV1>>(services.Body.Items);
            var syncTime = DateTime.UtcNow;

            foreach (var item in dtoitems)
            {
                item.LatestSyncDateUTC = syncTime;
                foreach (var ing in ingresses.Where(p => p.Metadata.NamespaceProperty == item.Namespace))
                {
                    var paths = ing.Spec.Rules.FirstOrDefault(q => q.Http.Paths.All(pp => pp.Backend.Service.Name == item.Name));
                    if (paths != null)
                    {
                        var IngressUrl = "http://" + paths.Host;
                        if (ing.Spec.Tls != null)
                        {
                            IngressUrl = "https://" + paths.Host;
                        }
                        item.Ingresses.Add(IngressUrl);
                    }
                }


                var vs = virtualservices.FirstOrDefault(p => p.Namespace == item.Namespace && p.Service == item.Name);
                if (vs != null)
                {
                    item.VirtualServiceUrl = "http://" + vs.Host;
                }
            }
            redisDicServices.UpSert(dtoitems);
            _logger.LogInformation(dtoitems.Count.ToString() + " Services have been synced");

            return Task.CompletedTask;


        }
    }
}