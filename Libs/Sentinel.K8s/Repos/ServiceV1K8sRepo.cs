using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Sentinel.Models.K8sDTOs;

namespace Sentinel.K8s.Repos
{
    public class ServiceV1K8sRepo
    {
        private readonly IKubernetesClient _k8sclient;
        private readonly IMapper _mapper;
        private readonly ILogger<ServiceV1K8sRepo> _logger;

        public ServiceV1K8sRepo(IKubernetesClient k8sclient, IMapper mapper, ILogger<ServiceV1K8sRepo> logger)
        {
            _k8sclient = k8sclient;
            _mapper = mapper;
            _logger = logger;
        }


        public IList<ServiceV1> GetAllServicesWithDetails()
        {
            // var services = new List<ServiceV1>();
            var servicesTask = _k8sclient.ApiClient.ListServiceForAllNamespacesWithHttpMessagesAsync();
            var ingressesTask = _k8sclient.ApiClient.ListIngressForAllNamespacesWithHttpMessagesAsync();
            var virtualservicesTask = _k8sclient.ListClusterCustomObjectAsync("networking.istio.io", "v1alpha3", "virtualservices");

            Task.WaitAll(servicesTask, ingressesTask, virtualservicesTask);

            var ingresses = ingressesTask.Result.Body.Items;
            var virtualservices = VirtualServiceV1.ConvertFromJTokenToVirtualServiceV1List(virtualservicesTask.Result);
            var services = servicesTask.Result;

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

            return dtoitems;
        }
    }
}