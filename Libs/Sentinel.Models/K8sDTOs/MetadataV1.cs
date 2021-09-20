using System.Collections.Generic;

using System;
using System.Collections.Generic;

namespace Sentinel.Models.K8sDTOs
{
    public class MetadataV1
    {
        public List<Label> Annotations { get; set; }
        public DateTime? CreationTime { get; set; }
        public int Generation { get; set; }
        public List<Label> Labels { get; set; }
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string ResourceVersion { get; set; }
        public string Uid { get; set; }
        public IList<OwnerReferenceV1> OwnerReferences { get; set; }
    }
}