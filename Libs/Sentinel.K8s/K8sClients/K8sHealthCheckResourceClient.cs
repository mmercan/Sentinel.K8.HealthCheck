using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Sentinel.Models.CRDs;
using Sentinel.Models.K8sDTOs;

namespace Sentinel.K8s.K8sClients
{
    public class K8sHealthCheckResourceClient
    {
        private readonly IKubernetesClient _k8sclient;
        private readonly IMapper _mapper;
        private readonly ILogger<K8sGeneralService> _logger;

        public K8sHealthCheckResourceClient(IKubernetesClient k8sclient, IMapper mapper, ILogger<K8sGeneralService> logger)
        {
            _k8sclient = k8sclient;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<HealthCheckResource?> GetHealthCheckResourceAsync(string Name, string Namespace)
        {
            var healthCheck = await _k8sclient.GetAsync<HealthCheckResource>(Name, Namespace);
            //healthCheck.Metadata.ResourceVersion
            return healthCheck;
        }

        public async Task<IList<HealthCheckResource>> GetAllHealthCheckResourcesAsync()
        {
            var healthChecks = await _k8sclient.ListAsync<HealthCheckResource>();
            return healthChecks;
        }



        public async Task<HealthCheckResource> UpdateStartusAsync(HealthCheckResource healthCheck, HealthCheckResource.HealthCheckResourceStatusPhase phase, DateTime? lastCheckTime = null)
        {
            return await UpdateStartusAsync(healthCheck, phase.ToString(), lastCheckTime);

        }



        public async Task<HealthCheckResource> UpdateStartusAsync(string Name, string Namespace, HealthCheckResource.HealthCheckResourceStatusPhase phase, DateTime? lastCheckTime = null)
        {
            return await UpdateStartusAsync(Name, Namespace, phase.ToString(), lastCheckTime);
        }

        public async Task<HealthCheckResource> UpdateStartusAsync(string Name, string Namespace, string phase, DateTime? lastCheckTime = null)
        {
            var healthCheck = await GetHealthCheckResourceAsync(Name, Namespace);
            if (healthCheck == null)
            {
                throw new Exception($"HealthCheckResource {Name} not found");
            }
            return await UpdateStartusAsync(healthCheck, phase, lastCheckTime);
        }

        public async Task<HealthCheckResource> UpdateStartusAsync(HealthCheckResource healthCheck, string status, DateTime? lastCheckTime = null)
        {
            if (healthCheck.Status == null)
            {
                healthCheck.Status = new HealthCheckResource.HealthCheckResourceStatus();
            }
            healthCheck.Status.Phase = status;
            if (lastCheckTime != null)
            {
                healthCheck.Status.LastCheckTime = lastCheckTime.Value.ToString();
            }
            await _k8sclient.UpdateStatusAsync(healthCheck);
            _logger.LogDebug("K8s HealthCheckResource {name} status updated to {status}", healthCheck.Metadata.Name, healthCheck.Status.Phase);
            return healthCheck;
        }
        // public async Task<HealthCheckResourceV1> GetHealthCheckResource(string name, string namespaceName)
        // {
        //     var healthCheckResource = await _k8sclient.ReadNamespacedCustomObjectAsync<HealthCheckResourceV1, HealthCheckResourceV1List>(
        //         "sentinel.io",
        //         "v1",
        //         namespaceName,
        //         "healthchecks",
        //         name);

        //     return healthCheckResource;
        // }
    }
}