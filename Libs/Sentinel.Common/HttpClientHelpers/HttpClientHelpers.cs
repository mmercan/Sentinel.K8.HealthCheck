
using Polly;
using Polly.Extensions.Http;

using Microsoft.Extensions.Configuration;

namespace Sentinel.Common.HttpClientHelpers
{
    public static class HttpClientHelpers
    {

        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
              .HandleTransientHttpError()
              .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
              .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
        }
    }
    public class CertMessageHandler : HttpClientHandler
    {
        public CertMessageHandler()
        {
            ClientCertificateOptions = ClientCertificateOption.Manual;
            var cert = HttpClientCertificateHelpers.GetCertFromcertThumbprint();
            if (cert != null)
            {
                ClientCertificates.Add(cert);
            }
        }
    }
}