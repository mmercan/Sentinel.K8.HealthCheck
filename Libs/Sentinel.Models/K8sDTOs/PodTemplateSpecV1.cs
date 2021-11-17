namespace Sentinel.Models.K8sDTOs
{
    public class PodTemplateSpecV1
    {
        public MetadataV1 Metadata { get; set; } = default!;
        public PodSpecV1 Spec { get; set; } = default!;
    }
}