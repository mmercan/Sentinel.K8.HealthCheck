using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Logging;

namespace Sentinel.K8s.K8sClients
{
    public class K8sGeneralService
    {
        private readonly IKubernetesClient _k8sclient;
        private readonly IMapper _mapper;
        private readonly ILogger<K8sGeneralService> _logger;


        public K8sServiceClient ServiceClient { get; }
        public K8sHealthCheckResourceClient HealthCheckResourceClient { get; }

        public K8sEventClient EventClient { get; }

        public K8sGeneralService(IKubernetesClient k8sclient, IMapper mapper, ILogger<K8sGeneralService> logger)
        {
            _k8sclient = k8sclient;
            _mapper = mapper;
            _logger = logger;

            ServiceClient = new K8sServiceClient(_k8sclient, mapper, logger);
            HealthCheckResourceClient = new K8sHealthCheckResourceClient(_k8sclient, mapper, logger);
            EventClient = new K8sEventClient(_k8sclient, mapper, logger);
        }

    }
}
