using System;

namespace Sentinel.Models.K8sDTOs
{
    public class DeploymentConditionV1
    {
        public DateTime? LastTransitionTime { get; set; }
        public DateTime? LastUpdateTime { get; set; }
        public string Message { get; set; } = default!;
        public string Reason { get; set; } = default!;
        public string Status { get; set; } = default!;
        public string Type { get; set; } = default!;
    }
}