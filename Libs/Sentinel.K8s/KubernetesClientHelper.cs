using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;

namespace Sentinel.K8s
{
    public static class KubernetesClientHelper
    {
        public static void SetTcpKeepAlives(k8s.Kubernetes client)
        {

            var realHandler = client.HttpMessageHandlers.FirstOrDefault(h => !(h is DelegatingHandler));
            if (!(realHandler is HttpClientHandler))
            {
                throw new ArgumentException("Expected HttpClientHandler");
            }

            var underlyingHandlerProperty = realHandler.GetType().GetField("_underlyingHandler", BindingFlags.NonPublic | BindingFlags.Instance);
            if (underlyingHandlerProperty == null)
            {
                throw new NullReferenceException("Expected _underlyingHandler property not found.");
            }

            var underlyingHandler = underlyingHandlerProperty.GetValue(realHandler);
            if (underlyingHandler == null)
            {
                throw new NullReferenceException("_underlyingHandler is null.");
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