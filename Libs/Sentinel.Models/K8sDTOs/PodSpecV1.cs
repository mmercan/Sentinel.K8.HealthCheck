using System.Collections.Generic;

namespace Sentinel.Models.K8sDTOs
{
    public class PodSpecV1
    {
        public string PreemptionPolicy { get; set; }
        public int? Priority { get; set; }

        public string PriorityClassName { get; set; }

        public string RestartPolicy { get; set; }
        public string RuntimeClassName { get; set; }

        public string SchedulerName { get; set; }

        public string ServiceAccount { get; set; }
        public string ServiceAccountName { get; set; }
        public bool? ShareProcessNamespace { get; set; }
        public string Subdomain { get; set; }
        public long? TerminationGracePeriodSeconds { get; set; }

        public List<Label> NodeSelector { get; set; }
        public string NodeName { get; set; }
        public long? ActiveDeadlineSeconds { get; set; }
        public bool? AutomountServiceAccountToken { get; set; }

        public IList<ContainerV1> Containers { get; set; }
    }
}