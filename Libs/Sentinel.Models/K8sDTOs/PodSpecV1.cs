using System.Collections.Generic;

namespace Sentinel.Models.K8sDTOs
{
    public class PodSpecV1
    {
        public string PreemptionPolicy { get; set; } = default!;
        public int? Priority { get; set; }

        public string PriorityClassName { get; set; } = default!;

        public string RestartPolicy { get; set; } = default!;
        public string RuntimeClassName { get; set; } = default!;

        public string SchedulerName { get; set; } = default!;

        public string ServiceAccount { get; set; } = default!;
        public string ServiceAccountName { get; set; } = default!;
        public bool? ShareProcessNamespace { get; set; }
        public string Subdomain { get; set; } = default!;
        public long? TerminationGracePeriodSeconds { get; set; }

        public List<Label> NodeSelector { get; set; } = default!;
        public string NodeName { get; set; } = default!;
        public long? ActiveDeadlineSeconds { get; set; }
        public bool? AutomountServiceAccountToken { get; set; }

        public IList<ContainerV1> Containers { get; set; } = default!;
    }
}