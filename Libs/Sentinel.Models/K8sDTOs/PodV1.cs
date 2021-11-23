using System;
using System.Collections.Generic;

namespace Sentinel.Models.K8sDTOs
{
    public class PodV1
    {
        public string Name { get; set; } = default!;
        public string GenerateName { get; set; } = default!;
        public string Namespace { get; set; } = default!;
        public List<Label> Labels { get; set; } = default!;
        public DateTime CreationTime { get; set; }
        public List<Label> LabelSelector { get; set; } = default!;

        public List<Label> Annotations { get; set; } = default!;

        public string ClusterIP { get; set; } = default!;

        public string Uid { get; set; } = default!;

        public List<OwnerReferenceV1> OwnerReferences { get; set; } = default!;

        public List<ContainerV1> Containers { get; set; } = default!;

        public string Status { get; set; } = default!;

        public string PodIP { get; set; } = default!;
        public DateTime StartTime { get; set; }

        public string NodeName { get; set; } = default!;

        public List<string> Images { get; set; } = default!;
        public List<string> InternalEndpoints { get; set; } = default!;
        public List<string> ExternalEndpoints { get; set; } = default!;
    }
}