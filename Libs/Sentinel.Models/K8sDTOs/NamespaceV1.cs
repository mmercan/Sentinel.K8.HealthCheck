using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sentinel.Models.K8sDTOs
{
    public class NamespaceV1
    {
        public string Uid { get; set; } = default!;

        [Key]
        public string Name { get; set; } = default!;
        public List<Label> Labels { get; set; } = default!;
        public DateTime CreationTime { get; set; }
        public string Status { get; set; } = default!;

        public DateTime LatestSyncDateUTC { get; set; }

        public int DeploymentCount { get; set; }
        public int ServiceCount { get; set; }
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
    }
}