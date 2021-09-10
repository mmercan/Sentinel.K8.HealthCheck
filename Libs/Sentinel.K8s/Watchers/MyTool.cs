using k8s;

namespace Sentinel.K8s.Watchers
{
    public class MyTool
    {
        private KubernetesClientConfiguration _clientConfig;

        public MyTool(KubernetesClientConfiguration clientConfig)
        {
            _clientConfig = clientConfig;
        }
    }
}