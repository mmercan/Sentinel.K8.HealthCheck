using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace Sentinel.Models.K8sDTOs
{
    public class VirtualServiceV1
    {

        [Key]
        public string NameandNamespace { get { return Name + "." + Namespace; } }
        public string Host { get; set; } = default!;
        public string Service { get; set; } = default!;
        public string? Port { get; set; } = default!;

        public string Name { get; set; } = default!;
        public string Namespace { get; set; } = default!;
        public string? GatewayName { get; set; } = default!;


        public static VirtualServiceV1 ConvertFromJTokenToVirtualServiceV1(JToken jitem)
        {
            VirtualServiceV1 virtualService = new VirtualServiceV1();
            virtualService.Host = jitem.SelectToken("spec.hosts[0]").ToString();
            virtualService.GatewayName = jitem.SelectToken("spec.gateways[0]")?.ToString();
            var service = jitem.SelectToken("spec.http[0].route[0].destination.host")?.ToString();
            if (service != null)
            {
                virtualService.Service = service;
            }
            else
            {
                //TODO what if protocol is not http (TCP or TLS etc..) 
            }
            // virtualService.Service = jitem.SelectToken("spec.http[0].route[0].destination.host").ToString();
            virtualService.Port = jitem.SelectToken("spec.http[0].route[0].destination.port.number")?.ToString();

            virtualService.Name = jitem.SelectToken("metadata.name").ToString();
            virtualService.Namespace = jitem.SelectToken("metadata.namespace").ToString();
            return virtualService;

        }

        public static List<VirtualServiceV1> ConvertFromJTokenToVirtualServiceV1List(List<JToken> jitemlist)
        {
            List<VirtualServiceV1> virtualServiceList = new List<VirtualServiceV1>();
            if (jitemlist != null)
            {
                foreach (var item in jitemlist)
                {
                    if (item != null)
                    {
                        var virtualService = VirtualServiceV1.ConvertFromJTokenToVirtualServiceV1(item);
                        virtualServiceList.Add(virtualService);
                    }
                }
            }
            return virtualServiceList;
        }
    }
}