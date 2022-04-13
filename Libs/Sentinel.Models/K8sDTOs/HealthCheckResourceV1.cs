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

        public MetadataV1 Metadata { get; set; } = default!;
        public List<Label> Annotations { get; set; } = default!;
        public List<Label> Labels { get; set; } = default!;
        public DateTime CreationTime { get; set; }
        public string Uid { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Namespace { get; set; } = default!;

        public string Schedule { get; set; } = default!;

        public DateTime LatestSyncDateUTC { get; set; }

        public HealthCheckResourceSpecV1 Spec { get; set; } = default!;

        public HealthCheckResourceStatusV1 Status { get; set; } = default!;

        public ServiceV1 RelatedService { get; set; } = default!;

        public static string Group = "sentinel.mercan.io";
        public static string Kind = "HealthCheck";
        public static string ApiVersion = "v1";
        public static string PluralName = "healthchecks";


    }

    public class HealthCheckResourceSpecV1 : HealthCheckResource.HealthCheckResourceSpec
    {

    }

    public class HealthCheckResourceStatusV1 : HealthCheckResource.HealthCheckResourceStatus
    {

    }
}