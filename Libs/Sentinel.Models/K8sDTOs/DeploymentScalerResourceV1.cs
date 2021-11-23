using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Sentinel.Models.CRDs;
using Sentinel.Models.Scheduler;

namespace Sentinel.Models.K8sDTOs
{
    public class DeploymentScalerResourceV1 : IScheduledTask
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

        public DeploymentScalerResourceSpecV1 Spec { get; set; } = default!;

        public DeploymentScalerResourceStatusV1 Status { get; set; } = default!;
    }

    public class DeploymentScalerResourceSpecV1 : DeploymentScalerResource.DeploymentScalerResourceSpec
    {

    }

    public class DeploymentScalerResourceStatusV1 : DeploymentScalerResource.DeploymentScalerResourceStatus
    {

    }
}