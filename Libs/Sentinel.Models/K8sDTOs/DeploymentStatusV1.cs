using System.Collections.Generic;

namespace Sentinel.Models.K8sDTOs
{
    public class DeploymentStatusV1
    {
        public int? AvailableReplicas { get; set; }
        public int? CollisionCount { get; set; }
        public IList<DeploymentConditionV1> Conditions { get; set; }
        public long? ObservedGeneration { get; set; }
        public int? ReadyReplicas { get; set; }
        public int? Replicas { get; set; }
        public int? UnavailableReplicas { get; set; }
        public int? UpdatedReplicas { get; set; }
    }
}