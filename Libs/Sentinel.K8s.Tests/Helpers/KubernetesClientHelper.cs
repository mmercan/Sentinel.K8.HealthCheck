using k8s;

namespace Sentinel.K8s.Tests.Helpers
{
    public static class KubernetesClientTestHelper
    {
        public static KubernetesClient GetKubernetesClient()
        {
            KubernetesClientConfiguration config = k8s.KubernetesClientConfiguration.BuildConfigFromConfigFile();
            KubernetesClient client = new KubernetesClient(config);
            return client;
        }

    }
}