using k8s;
using Sentinel.K8s;
using Sentinel.Tests.Helpers;

namespace Sentinel.Worker.Sync.TestsHelpers
{
    public class KubernetesClientTestHelper
    {
        public static KubernetesClient GetKubernetesClient()
        {
            KubernetesClientConfiguration config = k8s.KubernetesClientConfiguration.BuildConfigFromConfigFile();
            var logger = Helpers.GetLogger<KubernetesClient>();


            KubernetesClient client = new KubernetesClient(config, logger);
            return client;
        }
    }
}