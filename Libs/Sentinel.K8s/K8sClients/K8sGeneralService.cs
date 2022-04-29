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
        public readonly IKubernetesClient Client;
        private readonly IMapper _mapper;
        private readonly ILogger<K8sGeneralService> _logger;


        public K8sServiceClient ServiceClient { get; }
        public K8sHealthCheckResourceClient HealthCheckResourceClient { get; }

        public K8sEventClient EventClient { get; }

        public K8sGeneralService(IKubernetesClient k8sclient, IMapper mapper, ILogger<K8sGeneralService> logger)
        {
            Client = k8sclient;
            _mapper = mapper;
            _logger = logger;

            ServiceClient = new K8sServiceClient(Client, mapper, logger);
            HealthCheckResourceClient = new K8sHealthCheckResourceClient(Client, mapper, logger);
            EventClient = new K8sEventClient(Client, mapper, logger);
        }

    }
}
