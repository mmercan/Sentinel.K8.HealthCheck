using System.Collections.Generic;
using k8s;
using k8s.Models;
using Newtonsoft.Json;

namespace Sentinel.Models.CRDs
{


    [KubernetesEntity(Group = "sentinel.mercan.io", Kind = "HealthCheck", ApiVersion = "v1", PluralName = "healthchecks")]
    public class HealthCheckResource : CustomResource<HealthCheckResourceSpec, HealthCheckResourceStatus>
    {

        public override string ToString()
        {
            var labels = "{";
            if (Metadata != null && Metadata.Labels != null)
            {
                foreach (var kvp in Metadata.Labels)
                {
                    labels += kvp.Key + " : " + kvp.Value + ", ";
                }
            }
            labels = labels.TrimEnd(',', ' ') + "}";

            return $"{Metadata.Name} (Labels: {labels}), Spec: {Spec.Service}";
        }
    }

    public class HealthCheckResourceSpec
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

    [KubernetesEntity(Group = "sentinel.mercan.io", Kind = "HealthCheck", ApiVersion = "v1", PluralName = "healthchecks")]
    public class HealthCheckResourceStatus : V1Status
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