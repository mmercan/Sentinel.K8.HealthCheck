using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace Sentinel.Common.AuthServices
{
    public class AZAuthService
    {
        private readonly ILogger<AZAuthService> _logger;
        private readonly IOptions<AZAuthServiceSettings> _settingsOptions;
        private readonly IMemoryCache _memoryCache;

        // private static readonly HttpClient client = new HttpClient();
        public AZAuthService(ILogger<AZAuthService> logger, IOptions<AZAuthServiceSettings> settingsOptions, IMemoryCache memoryCache)
        {
            this._logger = logger;
            this._settingsOptions = settingsOptions;
            this._memoryCache = memoryCache;
        }

        public Task<string> AuthenticateAsync()
        {
            return AuthenticateAsync(_settingsOptions.Value);
        }

        public async Task<string> AuthenticateAsync(AZAuthServiceSettings authsettings)
        {
            if (authsettings == null)
            {
                throw new ArgumentNullException("settingsOptions");
            }
            var setting = authsettings;

            if (string.IsNullOrEmpty(setting.Secret))
            {
                throw new ArgumentNullException("setting.Secret");
            }
            _logger.LogInformation(setting.Secret.Length + " Chars on Secret");

            CacheToken? token;
            bool isExist = _memoryCache.TryGetValue("token", out token);
            if (isExist)
            {
                _logger.LogInformation("expitres : " + token?.ExpiresOn.ToUniversalTime().ToString() + " Now : " + DateTime.UtcNow);
            }

            if (!isExist || DateTime.UtcNow.AddMinutes(1) > token?.ExpiresOn.ToUniversalTime())
            {
                _logger.LogInformation("token is expired or missing download, getting token ...");
                token = await downloadToken(setting);
            }
            else if (token != null)
            {
                var totalmin = (DateTime.UtcNow - token.ExpiresOn).TotalMinutes;
                _logger.LogInformation("used cached token expires in " + totalmin.ToString() + " Minutes  at UTC " + token.ExpiresOn.ToString());
            }

            if (token?.Token == null)
            {
                throw new Exception("token is null");
            }
            else
            {
                return token.Token;
            }
        }

        private async Task<CacheToken?> downloadToken(AZAuthServiceSettings setting)
        {
            //  var url = "https://login.microsoftonline.com/" + setting.TenantId + "/oauth2/token?resource=" + setting.ClientId;

            Uri baseUri = new Uri("https://login.microsoftonline.com/");
            Uri url = new Uri(baseUri, setting.TenantId + "/oauth2/token?resource=" + setting.ClientId);

            var nvc = new List<KeyValuePair<string, string>>();
            nvc.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            nvc.Add(new KeyValuePair<string, string>("client_id", setting.ClientId));
            nvc.Add(new KeyValuePair<string, string>("client_secret", setting.Secret));
            nvc.Add(new KeyValuePair<string, string>("resource", setting.ClientId));
            var client = new HttpClient();
            var req = new HttpRequestMessage(HttpMethod.Post, url) { Content = new FormUrlEncodedContent(nvc) };

            var res = await client.SendAsync(req);

            _logger.LogInformation("Az Auth Status Code " + res.StatusCode.ToString());

            var result = res.Content.ReadAsStringAsync();
            result.Wait();

            var s = Newtonsoft.Json.JsonConvert.DeserializeObject<AZToken>(result.Result);
            var token = s?.AccessToken;

            _logger.LogInformation(token);

            var date = DateTime.UtcNow;
            var expires_on = s?.ExpiresOn;
            if (expires_on != null)
            {
                date = convertDatetime(expires_on);
            }
            if (!string.IsNullOrWhiteSpace(token))
            {
                _logger.LogInformation("Token expires on (UTC) " + date.ToString());
                var ctoken = new CacheToken { ExpiresOn = date, Token = token };
                cacheToken(ctoken);
                return ctoken;
            }
            else { return default; }

        }

        private void cacheToken(CacheToken token)
        {
            // var cacheEntryOptions = new MemoryCacheEntryOptions()
            //     .SetSlidingExpiration(TimeSpan.FromMinutes(15));
            // memoryCache.Set("expireson", expireson);

            var cachetoken = new CacheToken { Token = token.Token, ExpiresOn = token.ExpiresOn };
            _memoryCache.Set("token", cachetoken);

        }

        private DateTime convertDatetime(string unixdate)
        {
            int unixdatenumber = 0;
            if (Int32.TryParse(unixdate, result: out unixdatenumber))
            {
                System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                dtDateTime = dtDateTime.AddSeconds(unixdatenumber).ToUniversalTime();
                return dtDateTime;
            }
            else
            {
                return DateTime.Now;
            }
        }
    }

    public class CacheToken
    {
        public string Token { get; set; } = default!;
        public DateTime ExpiresOn { get; set; }
    }
    public class AZAuthServiceSettings
    {
        public string ClientId { get; set; } = default!;
        public string TenantId { get; set; } = default!;
        public string Secret { get; set; } = default!;
    }
    public class AZToken
    {
        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set; } = default!;


        [JsonProperty(PropertyName = "expires_in")]
        public string ExpiresIn { get; set; } = default!;


        [JsonProperty(PropertyName = "expires_on")]
        public string ExpiresOn { get; set; } = default!;


        [JsonProperty(PropertyName = "not_before")]
        public string NotBefore { get; set; } = default!;


        [JsonProperty(PropertyName = "resource")]
        public string Resource { get; set; } = default!;



        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set; } = default!;
    }


}