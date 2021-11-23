using System.Collections.Generic;

namespace Sentinel.Models.K8sDTOs
{
    public class ContainerV1
    {
        public ProbeV1 LivenessProbe { get; set; } = default!;
        public ProbeV1 ReadinessProbe { get; set; } = default!;
        public ProbeV1 StartupProbe { get; set; } = default!;

        public string StartupProbeV1 { get; set; } = default!;
        public string ReadinessProbeV1 { get; set; } = default!;
        public string LivenessProbeV1 { get; set; } = default!;

        public IList<ContainerPortV1> Ports { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string ImagePullPolicy { get; set; } = default!;
        public string Image { get; set; } = default!;
        public string ConfigMapRef { get; set; } = default!;
        public string SecretMapRef { get; set; } = default!;
        public IList<string> Command { get; set; } = default!;
        public IList<string> Args { get; set; } = default!;


        public string WorkingDir { get; set; } = default!;
    }
}