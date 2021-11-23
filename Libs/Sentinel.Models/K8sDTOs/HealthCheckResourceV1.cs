using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Sentinel.Models.CRDs;
using Sentinel.Models.Scheduler;

namespace Sentinel.Models.K8sDTOs
{
    public class HealthCheckResourceV1 : IScheduledTask
    {

        [Key]
        public string Key { get { return Name + "." + Namespace; } }
        public List<Label> Annotations { get; set; } = default!;
        public List<Label> Labels { get; set; } = default!;
        public DateTime CreationTime { get; set; }
        public string Uid { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Namespace { get; set; } = default!;

        public string Schedule { get; set; } = default!;

        public DateTime LatestSyncDateUTC { get; set; }

        public HealthCheckResourceSpecV1 Specs { get; set; } = default!;

        public HealthCheckResourceStatusV1 Status { get; set; } = default!;
    }

    public class HealthCheckResourceSpecV1 : HealthCheckResource.HealthCheckResourceSpec
    {

    }

    public class HealthCheckResourceStatusV1 : HealthCheckResource.HealthCheckResourceStatus
    {

    }
}