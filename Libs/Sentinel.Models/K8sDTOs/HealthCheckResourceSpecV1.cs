using Newtonsoft.Json;

namespace Sentinel.Models.K8sDTOs
{
    public class HealthCheckResourceSpecV1
    {
        [JsonProperty(PropertyName = "service")]
        public string Service { get; set; }

        [JsonProperty(PropertyName = "crontab")]
        public string Crontab { get; set; }

        [JsonProperty(PropertyName = "isaliveUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string IsaliveUrl { get; set; }

        [JsonProperty(PropertyName = "isaliveandwellUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string IsaliveandwellUrl { get; set; }

        [JsonProperty(PropertyName = "clientid", NullValueHandling = NullValueHandling.Ignore)]
        public string ClientId { get; set; }

        [JsonProperty(PropertyName = "cert", NullValueHandling = NullValueHandling.Ignore)]
        public string Cert { get; set; }
    }
}