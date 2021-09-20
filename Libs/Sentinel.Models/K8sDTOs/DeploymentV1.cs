using System;
using System.ComponentModel.DataAnnotations;

namespace Sentinel.Models.K8sDTOs
{
    public class DeploymentV1
    {

        [Key]
        public string NameandNamespace { get { return Name + "." + Namespace; } }
        public string Name { get; set; }
        public string Namespace { get; set; }

        public string Kind { get; set; }
        public MetadataV1 Metadata { get; set; }
        public DeploymentSpecV1 Spec { get; set; }
        public DeploymentStatusV1 Status { get; set; }

        public DateTime SyncDate { get; set; }
        public bool Deleted { get; set; }

    }
}