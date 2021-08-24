using System.Collections.Generic;
using k8s;
using k8s.Models;

namespace Sentinel.Models.K8s.Entities
{
    public class EntityList<T> : KubernetesObject
     where T : IKubernetesObject<V1ObjectMeta>
    {
        public V1ListMeta Metadata { get; set; } = new();

        public IList<T> Items { get; set; } = new List<T>();
    }
}