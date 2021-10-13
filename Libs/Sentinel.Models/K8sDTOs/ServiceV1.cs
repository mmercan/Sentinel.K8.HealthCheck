using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sentinel.Models.K8sDTOs
{
    public class ServiceV1
    {
        [Key]
        public string NameandNamespace { get { return Name + "." + Namespace; } }
        public string Uid { get; set; }

        public string Name { get; set; }
        public string Namespace { get; set; }
        public List<Label> Labels { get; set; }
        public DateTime CreationTime { get; set; }
        public List<Label> LabelSelector { get; set; }

        public string LabelSelectorString { get; set; }

        public List<Label> Annotations { get; set; }
        public string Type { get; set; }
        public string SessionAffinity { get; set; }

        public string ClusterIP { get; set; }
        public List<string> InternalEndpoints { get; set; }
        public List<string> ExternalEndpoints { get; set; }

        public string IngressUrl { get; set; }

        public string VirtualServiceUrl { get; set; }

        public DateTime LatestSyncDateUTC { get; set; }
        public bool Deleted { get; set; }

        public string ServiceApiVersion { get; set; }
        public string ServiceResourceVersion { get; set; }

        public ProbeV1 LivenessProbe { get; set; }
        public ProbeV1 ReadinessProbe { get; set; }
        public ProbeV1 StartupProbe { get; set; }

        public string CronDescription { get; set; }
        public string CronTab { get; set; }
        public string CronTabException { get; set; }
    }
}