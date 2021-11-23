using System;
using System.Collections.Generic;
using System.Text;
using k8s;
using k8s.Models;
using Newtonsoft.Json;
using Sentinel.Models.K8s;
using Sentinel.Models.K8s.Entities;

namespace Sentinel.Models.CRDs
{

    [KubernetesEntity(Group = "sentinel.mercan.io", Kind = "DeploymentScaler", ApiVersion = "v1", PluralName = "deploymentscalers")]
    public class DeploymentScalerResourceList : CustomResource<DeploymentScalerResource.DeploymentScalerResourceSpec, DeploymentScalerResource.DeploymentScalerResourceStatus>
    {

        [JsonProperty(PropertyName = "items")]
        public IList<DeploymentScalerResource> Items { get; set; } = default!;

    }


    [KubernetesEntity(Group = "sentinel.mercan.io", Kind = "DeploymentScaler", ApiVersion = "v1", PluralName = "deploymentscalers")]
    [EntityScope(EntityScope.Namespaced)]
    public class DeploymentScalerResource : CustomResource<DeploymentScalerResource.DeploymentScalerResourceSpec, DeploymentScalerResource.DeploymentScalerResourceStatus>
    {
        public override string ToString()
        {
            StringBuilder labelsbld = new StringBuilder();
            labelsbld.Append('{');
            if (Metadata != null && Metadata.Labels != null)
            {
                foreach (var kvp in Metadata.Labels)
                {
                    labelsbld.Append(kvp.Key + " : " + kvp.Value + ", ");
                }
            }
            var labels = labelsbld.ToString().TrimEnd(',', ' ') + "}";
            return $"{Metadata?.Name} (Labels: {labels}), Spec: {Spec.Deployment}";
        }

        public DateTime SyncDate { get; set; } = default!;

        public class DeploymentScalerResourceSpec
        {
            [JsonProperty(PropertyName = "deployment")]
            public string Deployment { get; set; } = default!;

            [JsonProperty(PropertyName = "crontab")]
            public string Crontab { get; set; } = default!;

            [JsonProperty(PropertyName = "replicas")]
            public int Replicas { get; set; } = default!;

        }

        public class DeploymentScalerResourceStatus : V1Status
        {
            [JsonProperty(PropertyName = "phase")]
            public string Phase { get; set; } = default!;

            [JsonProperty(PropertyName = "lastFailureTime")]
            public string LastFailureTime { get; set; } = default!;

            [JsonProperty(PropertyName = "lastCheckTime")]
            public string LastCheckTime { get; set; } = default!;

            [JsonProperty(PropertyName = "replicas")]
            public string Replicas { get; set; } = default!;

            [JsonProperty(PropertyName = "labelSelector")]
            public string LabelSelector { get; set; } = default!;

        }
    }
}