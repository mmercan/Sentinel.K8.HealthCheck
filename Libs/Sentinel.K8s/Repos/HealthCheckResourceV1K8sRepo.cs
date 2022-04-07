using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Sentinel.Models.CRDs;
using Sentinel.Models.K8sDTOs;

namespace Sentinel.K8s.Repos
{
    public class HealthCheckResourceV1K8sRepo
    {
        private readonly IKubernetesClient _k8sclient;
        private readonly IMapper _mapper;
        private readonly ILogger<HealthCheckResourceV1K8sRepo> _logger;

        public HealthCheckResourceV1K8sRepo(IKubernetesClient k8sclient, IMapper mapper, ILogger<HealthCheckResourceV1K8sRepo> logger)
        {
            _k8sclient = k8sclient;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<HealthCheckResource?> GetHealthCheckResourceAsync(string Name, string Namespace)
        {
            var healthCheck = await _k8sclient.GetAsync<HealthCheckResource>(Name, Namespace);
            return healthCheck;
        }

        public async Task<IList<HealthCheckResource>> GetAllHealthCheckResourcesAsync()
        {
            var healthChecks = await _k8sclient.ListAsync<HealthCheckResource>();
            return healthChecks;
        }


        public async Task<HealthCheckResource> UpdateStartusAsync(string Name, string Namespace, HealthCheckResource.HealthCheckResourceStatus status)
        {
            var healthChecks = await GetHealthCheckResourceAsync(Name, Namespace);
            healthChecks.Status = status;
            await _k8sclient.UpdateStatusAsync(healthChecks);
            return healthChecks;
        }


        public async Task<HealthCheckResource> UpdateStartusAsync(HealthCheckResource healthCheck, HealthCheckResource.HealthCheckResourceStatus status)
        {

            healthCheck.Status = status;
            await _k8sclient.UpdateStatusAsync(healthCheck);
            return healthCheck;
        }

        public async Task<HealthCheckResource> UpdateStartusAsync(HealthCheckResource healthCheck, HealthCheckResource.HealthCheckResourceStatusPhase phase, DateTime? lastCheckTime = null)
        {

            if (healthCheck.Status == null)
            {
                healthCheck.Status = new HealthCheckResource.HealthCheckResourceStatus();
            }
            healthCheck.Status.Phase = phase.ToString();
            if (lastCheckTime != null)
            {
                healthCheck.Status.LastCheckTime = lastCheckTime.Value.ToLongDateString();
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