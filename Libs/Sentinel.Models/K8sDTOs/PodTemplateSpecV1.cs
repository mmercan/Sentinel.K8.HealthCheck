namespace Sentinel.Models.K8sDTOs
{
    public class PodTemplateSpecV1
    {
        public MetadataV1 Metadata { get; set; }
        public PodSpecV1 Spec { get; set; }
    }
}