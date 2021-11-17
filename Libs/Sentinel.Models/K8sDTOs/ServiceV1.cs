using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sentinel.Models.K8sDTOs
{
    public class ServiceV1
    {
        public ServiceV1()
        {
            Ingresses = new List<string>();
        }

        [Key]
        public string NameandNamespace { get { return Name + "." + Namespace; } }
        public string Uid { get; set; } = default!;

        public string Name { get; set; } = default!;
        public string Namespace { get; set; } = default!;
        public List<Label> Labels { get; set; } = default!;
        public DateTime CreationTime { get; set; }
        public List<Label> LabelSelector { get; set; } = default!;

        public string LabelSelectorString { get; set; } = default!;

        public List<Label> Annotations { get; set; } = default!;
        public string Type { get; set; } = default!;
        public string SessionAffinity { get; set; } = default!;

        public string ClusterIP { get; set; } = default!;
        public List<string> InternalEndpoints { get; set; } = default!;
        public List<string> ExternalEndpoints { get; set; } = default!;

        public IList<string> Ingresses { get; set; } = default!;

        public string VirtualServiceUrl { get; set; } = default!;

        public DateTime LatestSyncDateUTC { get; set; } = default!;
        public bool Deleted { get; set; }

        public string ServiceApiVersion { get; set; } = default!;
        public string ServiceResourceVersion { get; set; } = default!;

        public ProbeV1 LivenessProbe { get; set; } = default!;
        public ProbeV1 ReadinessProbe { get; set; } = default!;
        public ProbeV1 StartupProbe { get; set; } = default!;

        public string CronDescription { get; set; } = default!;
        public string CronTab { get; set; } = default!;
        public string CronTabException { get; set; } = default!;
    }
}