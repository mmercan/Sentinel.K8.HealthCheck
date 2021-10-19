using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Sentinel.Models.K8sDTOs
{
    public class HealthCheckResourceV1
    {

        [Key]
        public string Key { get { return Name + "." + Namespace; } }
        public List<Label> Annotations { get; set; }
        public List<Label> Labels { get; set; }
        public DateTime CreationTime { get; set; }
        //  public string Type { get; set; }
        public string Uid { get; set; }
        public string Name { get; set; }
        public string Namespace { get; set; }
        public DateTime LatestSyncDateUTC { get; set; }

        public HealthCheckResourceSpecV1 Specs { get; set; }

        public HealthCheckResourceStatusV1 Status { get; set; }
    }
}