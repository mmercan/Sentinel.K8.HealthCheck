using System;

namespace Sentinel.Models.K8sDTOs
{
    public class DeploymentConditionV1
    {
        public DateTime? LastTransitionTime { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public string Message { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; }
        public string Type { get; set; }
    }
}