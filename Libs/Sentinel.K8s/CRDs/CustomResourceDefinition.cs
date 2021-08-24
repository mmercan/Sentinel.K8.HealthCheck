using System.Collections.Generic;
using k8s;
using k8s.Models;
using Newtonsoft.Json;

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "This is just an example.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "This is just an example.")]

namespace Sentinel.K8s.CRDs
{
    public class CustomResourceDefinition
    {
        public string Version { get; set; }

        public string Group { get; set; }

        public string PluralName { get; set; }

        public string Kind { get; set; }

        public string Namespace { get; set; }
    }


    // IKubernetesObject<V1ObjectMeta>
    public abstract class CustomResource : IKubernetesObject<V1ObjectMeta>
    {

        [JsonProperty(PropertyName = "apiVersion")]
        public string ApiVersion { get; set; }


        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; }

        [JsonProperty(PropertyName = "metadata")]
        public V1ObjectMeta Metadata { get; set; }
    }

    public abstract class CustomResource<TSpec, TStatus> : CustomResource
    {

        [JsonProperty(PropertyName = "spec")]
        public TSpec Spec { get; set; }

        [JsonProperty(PropertyName = "status")]
        public TStatus Status { get; set; }
    }

    public class CustomResourceList<T> : KubernetesObject
    where T : CustomResource
    {
        public V1ListMeta Metadata { get; set; }
        public List<T> Items { get; set; }
    }
}