using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sentinel.Common.AuthServices;

namespace Sentinel.Common.HttpClientServices
{
    public class DownloadJsonService
    {
        private readonly HttpClient _client;
        private readonly ILogger<DownloadJsonService> _logger;
        private readonly IConfiguration _configuration;
        private readonly AZAuthService _azAuthService;

        public DownloadJsonService(HttpClient client, ILogger<DownloadJsonService> logger, IConfiguration configuration, AZAuthService azAuthService)
        {
            _client = client;
            _logger = logger;
            _configuration = configuration;
            _azAuthService = azAuthService;
        }

        private async Task<HttpResponseMessage> DownloadAsync(HttpClient client, Uri url)
        {
            _logger.LogInformation("Uri is : " + url.ToString());
            var getitem = await _client.GetAsync(url);

            _logger.LogInformation("GetAsync Completed " + url.ToString() + " with " + getitem.StatusCode.ToString());

            if (!getitem.IsSuccessStatusCode)
            {
                _logger.LogInformation(getitem.StatusCode.ToString() + " ");
            }
            else if (getitem.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogError(url.ToString() + " Unauthorized");
            }
            var status = getitem.StatusCode.ToString();
            var content = await getitem.Content.ReadAsStringAsync();
            return getitem;
            // return new IsAliveAndWellResult { Result = content, Status = status, IsSuccessStatusCode = isSuccessStatusCode, CheckedUrl = url.AbsoluteUri };
        }

        public async Task<HttpResponseMessage> DownloadAsync(Uri url, AZAuthServiceSettings azAuthSettings)
        {
            await authenticate(_client, azAuthSettings);
            return await DownloadAsync(_client, url);
        }


        public async Task<HttpResponseMessage> DownloadAsync(Uri url, string certThumbprint)
        {

            return await DownloadAsync(_client, url);
        }


        private async Task authenticate(HttpClient client, AZAuthServiceSettings azAuthSettings)
        {
            _logger.LogInformation("Auth is Started");
            string bearerToken = await _azAuthService.Authenticate(azAuthSettings);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }


        private async Task authenticate(HttpClient client)
        {
            _logger.LogInformation("Auth is Started");
            string bearerToken = await _azAuthService.Authenticate();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

    }
}