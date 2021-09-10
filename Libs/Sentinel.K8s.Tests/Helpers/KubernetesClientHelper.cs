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
            var logger = GetLogger<KubernetesClient>();


            KubernetesClient client = new KubernetesClient(config, logger);
            return client;
        }


        public static ILogger<T> GetLogger<T>()
        {

            var serviceProvider = new ServiceCollection()
           .AddLogging()
           .BuildServiceProvider();

            var factory = serviceProvider.GetService<ILoggerFactory>();

            var logger = factory.CreateLogger<T>();
            return logger;
        }
        public static K8sEventOps GetK8sEventOps()
        {
            var client = GetKubernetesClient();
            var logger = GetLogger<K8sEventOps>();
            K8sEventOps ops = new K8sEventOps(client, logger);
            return ops;
        }
    }
}