using Newtonsoft.Json;

namespace Sentinel.Models.K8sDTOs
{
    public class HealthCheckResourceStatusV1
    {
        [JsonProperty(PropertyName = "phase")]
        public string Phase { get; set; }

        [JsonProperty(PropertyName = "lastFailureTime")]
        public string LastFailureTime { get; set; }

        [JsonProperty(PropertyName = "lastCheckTime")]
        public string LastCheckTime { get; set; }

        [JsonProperty(PropertyName = "replicas")]
        public string Replicas { get; set; }

        [JsonProperty(PropertyName = "labelSelector")]
        public string LabelSelector { get; set; }
    }
}