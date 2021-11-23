using System.Collections.Generic;
using k8s;
using k8s.Models;
using Newtonsoft.Json;

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "CA1724:TypeNamesShouldNotMatchNamespaces", Justification = "This is just an example.")]
[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "This is just an example.")]

namespace Sentinel.Models.K8s
{
    public class CustomResourceDefinition
    {
        public string Version { get; set; } = default!;

        public string Group { get; set; } = default!;

        public string PluralName { get; set; } = default!;

        public string Kind { get; set; } = default!;

        public string Namespace { get; set; } = default!;
    }


    // IKubernetesObject<V1ObjectMeta>
    public abstract class CustomResource : IKubernetesObject<V1ObjectMeta>
    {

        [JsonProperty(PropertyName = "apiVersion")]
        public string ApiVersion { get; set; } = default!;


        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; } = default!;

        [JsonProperty(PropertyName = "metadata")]
        public V1ObjectMeta Metadata { get; set; } = default!;
    }

    public abstract class CustomResource<TSpec, TStatus> : CustomResource
    {

        [JsonProperty(PropertyName = "spec")]
        public TSpec Spec { get; set; } = default!;

        [JsonProperty(PropertyName = "status")]
        public TStatus Status { get; set; } = default!;
    }

    public class CustomResourceList<T> : KubernetesObject
    where T : CustomResource
    {
        public V1ListMeta Metadata { get; set; } = default!;
        public List<T> Items { get; set; } = default!;
    }
}