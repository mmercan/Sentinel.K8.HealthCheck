using System;
using System.ComponentModel.DataAnnotations;

namespace Sentinel.Models.K8sDTOs
{
    public class DeploymentV1
    {

        [Key]
        public string NameandNamespace { get { return Name + "." + Namespace; } }
        public string Name { get; set; } = default!;
        public string Namespace { get; set; } = default!;

        public string Kind { get; set; } = default!;
        public MetadataV1 Metadata { get; set; } = default!;
        public DeploymentSpecV1 Spec { get; set; } = default!;
        public DeploymentStatusV1 Status { get; set; } = default!;

        public DateTime SyncDate { get; set; }
        public bool Deleted { get; set; }

    }
}