using System;
using System.Collections.Generic;

namespace Sentinel.Models.K8sDTOs
{
    public class MetadataV1
    {
        public List<Label> Annotations { get; set; } = default!;
        public DateTime? CreationTime { get; set; }
        public int Generation { get; set; }
        public List<Label> Labels { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Namespace { get; set; } = default!;
        public string ResourceVersion { get; set; } = default!;
        public string Uid { get; set; } = default!;
        public IList<OwnerReferenceV1> OwnerReferences { get; set; } = default!;
    }
}