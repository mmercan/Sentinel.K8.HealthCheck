using System.Collections.Generic;

namespace Sentinel.Models.K8sDTOs
{
    public class ProbeV1
    {
        public IList<string> Exec { get; set; } = default!;
        public int FailureThreshold { get; set; }


        public string HttpGetHost { get; set; } = default!;
        public List<HttpHeaderV1> HttpGetHttpHeaders { get; set; } = default!;
        public string HttpGetPath { get; set; } = default!;
        public string HttpGetPort { get; set; } = default!;
        public string HttpGetScheme { get; set; } = default!;

        public int? InitialDelaySeconds { get; set; }
    }
}