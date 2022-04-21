using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Sentinel.Common.Middlewares;
using Sentinel.K8s;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Sentinel.K8s.K8sClients;

namespace Sentinel.K8s.ServiceDefinitions
{
    public class KubernetesServiceDefinition : IEndpointDefinition
    {
        public void DefineEndpoints(WebApplication app)
        {

        }

        public void DefineServices(IServiceCollection services, ConfigurationManager configuration)
        {
            if (configuration["RunOnCluster"] == "true") { services.AddSingleton<KubernetesClientConfiguration>(KubernetesClientConfiguration.InClusterConfig()); }
            else { services.AddSingleton<KubernetesClientConfiguration>(KubernetesClientConfiguration.BuildConfigFromConfigFile()); }


            services.AddSingleton<IKubernetesClient, KubernetesClient>();
            services.AddSingleton<KubernetesClient>();

            services.AddSingleton<K8sGeneralService>();
        }
    }
}