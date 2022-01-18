using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using k8s;

namespace Sentinel.K8s
{
    public static class KubernetesClientHelper
    {
        public static void SetTcpKeepAlives(IKubernetes iclient)
        {
            if (!(iclient is Kubernetes))
            {
                throw new ArgumentException("iclient is null or not Kubernetes object");
            }
            var client = iclient as k8s.Kubernetes;

            var realHandler = client?.HttpMessageHandlers.FirstOrDefault(h => !(h is DelegatingHandler));
            if (!(realHandler is HttpClientHandler))
            {
                throw new ArgumentException("Expected HttpClientHandler");
            }

            var underlyingHandlerProperty = realHandler.GetType().GetField("_underlyingHandler", BindingFlags.NonPublic | BindingFlags.Instance);
            if (underlyingHandlerProperty == null)
            {
                throw new ArgumentNullException("Expected _underlyingHandler property not found.");
            }

            var underlyingHandler = underlyingHandlerProperty.GetValue(realHandler);
            if (underlyingHandler == null)
            {
                throw new ArgumentNullException("_underlyingHandler is null.");
            }

            if (underlyingHandler is SocketsHttpHandler socketHandler)
            {
                // we reached the SocketsHttpHandler, enable the keepalive delay.
                socketHandler.KeepAlivePingDelay = TimeSpan.FromSeconds(10);
            }
            else
            {
                throw new ArgumentException($"Expected to find SocketsHttpHandler, but found: {underlyingHandler.GetType().Name}");
            }
        }
    }
}