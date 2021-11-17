namespace Sentinel.Models.K8sDTOs
{
    public class OwnerReferenceV1
    {
        public bool BlockOwnerDeletion { get; set; }
        public bool Controller { get; set; }
        public string Kind { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Uid { get; set; } = default!;
    }
}