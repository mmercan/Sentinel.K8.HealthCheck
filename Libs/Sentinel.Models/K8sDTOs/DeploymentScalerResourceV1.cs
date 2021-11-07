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
        public List<Label> Annotations { get; set; }
        public List<Label> Labels { get; set; }
        public DateTime CreationTime { get; set; }
        public string Uid { get; set; }
        public string Name { get; set; }
        public string Namespace { get; set; }

        public string Schedule { get; set; }

        public DateTime LatestSyncDateUTC { get; set; }

        public DeploymentScalerResourceSpecV1 Spec { get; set; }

        public DeploymentScalerResourceStatusV1 Status { get; set; }
    }

    public class DeploymentScalerResourceSpecV1 : DeploymentScalerResource.DeploymentScalerResourceSpec
    {

    }

    public class DeploymentScalerResourceStatusV1 : DeploymentScalerResource.DeploymentScalerResourceStatus
    {

    }
}