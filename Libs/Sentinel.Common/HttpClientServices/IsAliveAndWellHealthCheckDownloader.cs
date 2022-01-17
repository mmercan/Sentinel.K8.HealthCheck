using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sentinel.Common.AuthServices;
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

        public async Task<List<IsAliveAndWellResult>> DownloadAsync(ServiceV1 service)
        {
            var results = new List<IsAliveAndWellResult>();

            var uris = ExtractUriFromService(service);
            var isaliveandWellSuffix = ExtractIsAliveAndWellSuffix(service);
            await Authenticate(service, _client);

            foreach (var uri in uris)
            {
                Uri isaliveandwellUri = new Uri(uri, isaliveandWellSuffix);
                _logger.LogInformation($"Checking {isaliveandwellUri}");

                var result = await DownloadAsync(isaliveandwellUri);
                results.Add(result);
            }

            var isAliveAndWellHealthCheck = _configuration.GetSection("IsAliveAndWellHealthCheck");
            return await Task.FromResult<List<IsAliveAndWellResult>>(results);
        }

        public async Task<IsAliveAndWellResult> DownloadAsync(Uri uri)
        {
            var result = new IsAliveAndWellResult();
            result.CheckedUrl = uri.AbsoluteUri;

            try
            {
                var response = await _client.GetAsync(uri);
                result.Status = response.StatusCode.ToString();
                result.IsSuccessStatusCode = response.IsSuccessStatusCode;
                result.Result = await response.Content.ReadAsStringAsync();

                if ((response.StatusCode == HttpStatusCode.Unauthorized))
                {
                    _logger.LogInformation($"IsAliveAndWellHealthCheckDownloader : Unauthorized {uri}");
                }
                else
                {
                    _logger.LogInformation($"IsAliveAndWellHealthCheckDownloader : {uri} {result.Status}");
                }
            }
            catch (Exception ex)
            {
                result.IsSuccessStatusCode = false;
                result.Status = HttpStatusCode.InternalServerError.ToString();
                result.Exception = ex.Message;
                _logger.LogError(ex, $"IsAliveAndWellHealthCheckDownloader : Exception {uri}");
            }
            return await Task.FromResult(result);
        }

        public async Task Authenticate(ServiceV1 service, HttpClient client)
        {
            if (checkAuthentication(service))
            {
                _logger.LogInformation("Auth is Started");

                // TODO: add AZAuthServiceSettings for ClientId and ClientSecret

                string bearerToken = await _azAuthService.Authenticate();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            }
        }

        private bool checkAuthentication(ServiceV1 service)
        {
            return service.Annotations.Any(p => p.Key == "healthcheck/clientid");
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

        public string ExtractIsAliveAndWellSuffix(ServiceV1 service)
        {
            var suffix = service.Annotations.FirstOrDefault(p => p.Key == "healthcheck/isaliveandwell")?.Value;
            if (string.IsNullOrWhiteSpace(suffix))
            {
                suffix = _configuration["DefaultIsAliveAndWellSuffix"];
            }
            return suffix;
        }

    }

    public class IsAliveAndWellResult
    {
        public string Result { get; set; } = default!;
        public string Status { get; set; } = default!;
        public bool IsSuccessStatusCode { get; set; } = default!;
        public string CheckedUrl { get; set; } = default!;
        public string Exception { get; set; } = default!;
    }
}