using System;
using System.Collections.Generic;

namespace Sentinel.Models.K8sDTOs
{
    public class PodV1
    {
        public string Name { get; set; }
        public string GenerateName { get; set; }
        public string Namespace { get; set; }
        public List<Label> Labels { get; set; }
        public DateTime CreationTime { get; set; }
        public List<Label> LabelSelector { get; set; }

        public List<Label> Annotations { get; set; }

        public string ClusterIP { get; set; }

        public string Uid { get; set; }

        public List<OwnerReferenceV1> OwnerReferences { get; set; }

        public List<ContainerV1> Containers { get; set; }

        public string Status { get; set; }

        public string PodIP { get; set; }
        public DateTime StartTime { get; set; }

        public string NodeName { get; set; }

        public List<string> Images { get; set; }
        // public List<string> InternalEndpoints { get; set; }
        // public List<string> ExternalEndpoints { get; set; }
    }
}