using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace Sentinel.Models.K8sDTOs
{
    public class VirtualServiceV1
    {

        [Key]
        public string NameandNamespace { get { return Name + "." + Namespace; } }
        public string Host { get; set; }
        public string Service { get; set; }
        public string Port { get; set; }

        public string Name { get; set; }
        public string Namespace { get; set; }


        public static VirtualServiceV1 ConvertFromJTokenToVirtualServiceV1(JToken jitem)
        {
            VirtualServiceV1 virtualService = new VirtualServiceV1();
            virtualService.Host = jitem.SelectToken("spec.hosts[0]").ToString();
            virtualService.Service = jitem.SelectToken("spec.http[0].route[0].destination.host").ToString();
            virtualService.Port = jitem.SelectToken("spec.http[0].route[0].destination.port.number").ToString();

            virtualService.Name = jitem.SelectToken("metadata.name").ToString();
            virtualService.Namespace = jitem.SelectToken("metadata.namespace").ToString();
            return virtualService;

        }
    }
}