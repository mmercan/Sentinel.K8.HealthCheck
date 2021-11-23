using System.Diagnostics.CodeAnalysis;
using k8s;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sentinel.K8s.Watchers;

namespace Sentinel.K8s.Tests.Helpers
{
    public static class KubernetesClientTestHelper
    {
        public static KubernetesClient GetKubernetesClient()
        {
            KubernetesClientConfiguration config = k8s.KubernetesClientConfiguration.BuildConfigFromConfigFile();
            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<KubernetesClient>();


            KubernetesClient client = new KubernetesClient(config, logger);
            return client;
        }



        public static K8SEventOps GetK8SEventOps()
        {
            var client = GetKubernetesClient();
            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<K8SEventOps>();
            K8SEventOps ops = new K8SEventOps(client, logger);
            return ops;
        }
    }
}