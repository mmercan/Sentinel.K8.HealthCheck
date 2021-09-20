using System.Collections.Generic;

namespace Sentinel.Models.K8sDTOs
{
    public class ProbeV1
    {
        public IList<string> Exec { get; set; }
        public int FailureThreshold { get; set; }


        public string HttpGetHost { get; set; }
        public List<HttpHeaderV1> HttpGetHttpHeaders { get; set; }
        public string HttpGetPath { get; set; }
        public string HttpGetPort { get; set; }
        public string HttpGetScheme { get; set; }

        public int? InitialDelaySeconds { get; set; }
    }
}