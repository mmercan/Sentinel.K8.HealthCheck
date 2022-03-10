using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Sentinel.Common.AuthServices;

namespace Sentinel.Common.HttpClientServices
{
    public class DownloadService
    {
        private readonly HttpClient _client;
        private readonly ILogger<DownloadService> _logger;

        private readonly AZAuthService _azAuthService;

        public DownloadService(HttpClient client, ILogger<DownloadService> logger, AZAuthService azAuthService)
        {
            _client = client;
            _logger = logger;

            _azAuthService = azAuthService;
        }

        private async Task<HttpResponseMessage> DownloadAsync(HttpClient client, Uri url, bool retry = true)
        {
            _logger.LogInformation("Uri is : " + url.ToString());
            var getitem = await _client.GetAsync(url);

            _logger.LogInformation("GetAsync Completed " + url.ToString() + " with " + getitem.StatusCode.ToString());

            if (!getitem.IsSuccessStatusCode)
            {
                _logger.LogInformation(getitem.StatusCode.ToString() + " ");
            }
            if (getitem.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogError(url.ToString() + " Unauthorized");
                await authenticate(client);
                if (retry)
                {
                    return await DownloadAsync(client, url, false);
                }
            }
            // var status = getitem.StatusCode.ToString();
            // var content = await getitem.Content.ReadAsStringAsync();
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
            _logger.LogInformation("Download with a Cert Started");
            return await DownloadAsync(_client, url);
        }


        private async Task authenticate(HttpClient client, AZAuthServiceSettings azAuthSettings)
        {
            _logger.LogInformation("Auth is Started");
            string bearerToken = await _azAuthService.AuthenticateAsync(azAuthSettings);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }


        private async Task authenticate(HttpClient client)
        {
            _logger.LogInformation("Auth with default AZAuthServiceSettings is Started");
            string bearerToken = await _azAuthService.AuthenticateAsync();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
        }

    }
}