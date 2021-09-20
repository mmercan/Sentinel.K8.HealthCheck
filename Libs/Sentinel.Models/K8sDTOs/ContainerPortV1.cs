namespace Sentinel.Models.K8sDTOs
{
    public class ContainerPortV1
    {
        public int ContainerPort { get; set; }
        public string HostIP { get; set; }
        public int? HostPort { get; set; }
        public string Name { get; set; }
        public string Protocol { get; set; }
    }
}