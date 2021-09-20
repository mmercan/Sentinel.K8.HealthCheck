using System.Collections.Generic;

namespace Sentinel.Models.K8sDTOs
{
    public class ContainerV1
    {
        public ProbeV1 LivenessProbe { get; set; }
        public ProbeV1 ReadinessProbe { get; set; }
        public ProbeV1 StartupProbe { get; set; }

        public string StartupProbeV1 { get; set; }
        public string ReadinessProbeV1 { get; set; }
        public string LivenessProbeV1 { get; set; }

        public IList<ContainerPortV1> Ports { get; set; }
        public string Name { get; set; }
        public string ImagePullPolicy { get; set; }
        public string Image { get; set; }
        public string ConfigMapRef { get; set; }
        public string SecretMapRef { get; set; }
        public IList<string> Command { get; set; }
        public IList<string> Args { get; set; }


        public string WorkingDir { get; set; }
    }
}