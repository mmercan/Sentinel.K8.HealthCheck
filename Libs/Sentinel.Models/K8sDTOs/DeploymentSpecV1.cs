using System.Collections.Generic;

namespace Sentinel.Models.K8sDTOs
{
    public class DeploymentSpecV1
    {
        public int ProgressDeadlineSeconds { get; set; }
        public int Replicas { get; set; }
        public int RevisionHistoryLimit { get; set; }
        public List<Label> Selector { get; set; }
        public string SelectorString { get; set; }
        public PodTemplateSpecV1 Template { get; set; }
    }
}