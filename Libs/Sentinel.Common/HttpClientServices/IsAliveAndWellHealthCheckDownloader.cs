using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sentinel.Common.AuthServices;
using Sentinel.Models.HealthCheck;
using Sentinel.Models.K8sDTOs;

namespace Sentinel.Common.HttpClientServices
{
    public class IsAliveAndWellHealthCheckDownloader
    {
        private readonly HttpClient _client;
        private readonly ILogger<IsAliveAndWellHealthCheckDownloader> _logger;
        private readonly IConfiguration _configuration;
        private readonly AZAuthService _azAuthService;

        public IsAliveAndWellHealthCheckDownloader(HttpClient client, ILogger<IsAliveAndWellHealthCheckDownloader> logger, IConfiguration configuration, AZAuthService azAuthService)
        {
            _client = client;
            _logger = logger;
            _configuration = configuration;
            _azAuthService = azAuthService;
        }


        public async Task<IsAliveAndWellResult?> DownloadAsync(ServiceV1 service, HealthCheckResourceV1 healthcheck)
        {
            var results = new List<IsAliveAndWellResult>();
            var headers = new Dictionary<string, string>();

            string? IsaliveandwellPath = null;

            if (healthcheck?.Spec != null)
            {
                if (healthcheck.Spec.Cert != null && _configuration[healthcheck.Spec.Cert] != null)
                {
                    headers.Add("X-ARR-ClientCert", _configuration[healthcheck.Spec.Cert]);
                }
                if (healthcheck.Spec.ClientId != null)
                {
                    AZAuthServiceSettings setting = new AZAuthServiceSettings();
                    _configuration.GetSection(healthcheck.Spec.ClientId).Bind(setting);
                    if (setting?.ClientId != null)
                    {
                        var token = await _azAuthService.AuthenticateAsync(setting);
                        headers.Add(HttpRequestHeader.Authorization.ToString(), "Bearer " + token);
                    }
                }
                if (healthcheck.Spec.IsaliveandwellUrl != null)
                {
                    IsaliveandwellPath = healthcheck.Spec.IsaliveandwellUrl;
                }
                if (healthcheck.Spec.IsaliveUrl != null)
                {

                }
            }
            var isaliveandWellSuffix = ExtractIsAliveAndWellSuffix(service, IsaliveandwellPath);

            var uris = ExtractUriFromService(service);
            foreach (var uri in uris)
            {
                Uri isaliveandwellUri = new Uri(uri, isaliveandWellSuffix);
                _logger.LogInformation($"Checking {isaliveandwellUri}");

                var httpRequestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Get,
                    RequestUri = isaliveandwellUri,
                    // Headers = { { HttpRequestHeader.Accept.ToString(), "application/json" }, },
                    // Content = new StringContent(JsonConvert.SerializeObject(svm))
                };
                foreach (var item in headers)
                {
                    httpRequestMessage.Headers.Add(item.Key, item.Value);
                }

                var result = await DownloadAsync(httpRequestMessage);
                results.Add(result);

                if (result.Status != "Domain Record Not Found")
                {
                    break;
                }
            }
            if (results.Count == 1) return results[0];
            var successResult = results.FirstOrDefault(x => x.IsSuccessStatusCode);
            if (successResult is not null) return successResult;
            return results.FirstOrDefault();
        }

        public async Task<IsAliveAndWellResult> DownloadAsync(HttpRequestMessage message)
        {
            var result = new IsAliveAndWellResult();

            try
            {
                var response = await _client.SendAsync(message);
                result.Status = response.StatusCode.ToString();
                result.StatusCode = (int)response.StatusCode;
                result.IsSuccessStatusCode = response.IsSuccessStatusCode;
                result.Result = await response.Content.ReadAsStringAsync();
                if ((response.StatusCode == HttpStatusCode.Unauthorized))
                {
                    _logger.LogInformation($"IsAliveAndWellHealthCheckDownloader : Unauthorized {message.RequestUri}");
                }
                else if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    _logger.LogInformation($"IsAliveAndWellHealthCheckDownloader : ServiceUnavailable {message.RequestUri}");
                }
                else
                {
                    _logger.LogInformation($"IsAliveAndWellHealthCheckDownloader : {message.RequestUri} {result.Status}");
                }
                result.CheckedAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                result.Status = HttpStatusCode.InternalServerError.ToString();
                if (ex.Message.StartsWith("No such host is known."))
                {
                    result.Status = "Domain Record Not Found";
                }

                result.IsSuccessStatusCode = false;
                result.Exception = ex.Message;
                _logger.LogError(ex, $"IsAliveAndWellHealthCheckDownloader : Exception {message.RequestUri}");
            }

            result.Id = Guid.NewGuid().ToString();
            result.CheckedAt = DateTime.UtcNow;
            if (!string.IsNullOrEmpty(message?.RequestUri?.AbsoluteUri.ToString()))
            {
                result.CheckedUrl = message.RequestUri.AbsoluteUri.ToString();
            }
            return await Task.FromResult(result);
        }


        public bool isDomainExist(string address)
        {
            HttpRequestMessage message = new HttpRequestMessage(HttpMethod.Head, address);
            try
            {
                _client.SendAsync(message).Wait();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public List<Uri> ExtractUriFromService(ServiceV1 service)
        {
            List<Uri> endpoints = new List<Uri>();
            if (_configuration["RunOnCluster"] == "true")
            {
                foreach (var item in service.InternalEndpoints)
                {
                    endpoints.Add(new Uri("http://" + item));
                }
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(service.VirtualServiceUrl))
                {
                    endpoints.Add(new Uri(service.VirtualServiceUrl));
                }
                else if (service.Ingresses != null && service.Ingresses.Count > 0)
                {
                    foreach (var ingress in service.Ingresses)
                    {
                        endpoints.Add(new Uri(ingress));
                    }

                }
                else if (service.ExternalEndpoints != null && service.ExternalEndpoints.Count > 0)
                {
                    foreach (var item in service.ExternalEndpoints)
                    {
                        endpoints.Add(new Uri(item));
                    }
                }
                else
                {
                    foreach (var item in service.InternalEndpoints)
                    {
                        endpoints.Add(new Uri("http://" + item));
                    }
                }
            }
            return endpoints;
        }

        public string ExtractIsAliveAndWellSuffix(ServiceV1 service, string? healthcheckPath = null)
        {
            // var suffix = service.Annotations.FirstOrDefault(p => p.Key == "healthcheck/isaliveandwell")?.Value;
            if (string.IsNullOrWhiteSpace(healthcheckPath))
            {
                healthcheckPath = _configuration["DefaultIsAliveAndWellSuffix"];
            }
            return healthcheckPath;
        }

    }


}