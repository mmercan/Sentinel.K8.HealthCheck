namespace Sentinel.Models.K8sDTOs
{
    public class ContainerPortV1
    {
        public int ContainerPort { get; set; } = default!;
        public string HostIP { get; set; } = default!;
        public int? HostPort { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Protocol { get; set; } = default!;
    }
}