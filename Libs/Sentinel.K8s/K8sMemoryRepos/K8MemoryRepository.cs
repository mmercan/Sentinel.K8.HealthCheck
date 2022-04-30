using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sentinel.Models.CRDs;
using Sentinel.Models.K8sDTOs;

namespace Libs.Sentinel.K8s
{
    public class K8MemoryRepository
    {
        public K8MemoryRepository()
        {
            ServicesDic = new Dictionary<string, ServiceV1>();
            services = new List<ServiceV1>();
        }

        private IList<ServiceV1>? services = null;
        public IList<ServiceV1>? Services
        {
            get { return services; }
            set
            {
                services = value;
                if (value != null)
                {
                    ServicesDic = value.ToDictionary(p => p.NameandNamespace);
                }
            }
        }

        public IDictionary<string, ServiceV1>? ServicesDic { get; private set; }
        public IList<DeploymentV1>? Deployments { get; set; }

        public IList<NamespaceV1>? Namespaces { get; set; }

        public IList<HealthCheckResourceV1>? HealthChecks { get; set; }
        public IList<DeploymentScalerResourceV1>? DeploymentScalers { get; set; }
    }
}