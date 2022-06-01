using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace Sentinel.Models.K8sDTOs
{
    public class GatewayV1
    {

        [Key]
        public string NameandNamespace { get { return Name + "." + Namespace; } }
        public string IstioGatewayName { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Namespace { get; set; } = default!;

        public List<ServerPort> ServerPorts { get; set; } = default!;

        public class ServerPort
        {
            public string Host { get; set; } = default!;
            public string Port { get; set; } = default!;
            public bool Tls { get; set; }
        }

        public static GatewayV1 ConvertFromJTokenToGatewayV1(JToken jitem)
        {
            GatewayV1 gateway = new GatewayV1();

            gateway.IstioGatewayName = jitem.SelectToken("spec.selector.istio").ToString();

            IEnumerable<JToken> servers = jitem.SelectTokens("$.spec.servers[?(@.port.protocol == 'HTTP')]");
            gateway.ServerPorts = new List<ServerPort>();
            foreach (JToken item in servers)
            {
                var host = item.SelectToken("hosts[0]").ToString();
                var port = item.SelectToken("port.number").ToString();
                Int32.TryParse(port, out int portNumber);
                gateway.ServerPorts.Add(new ServerPort() { Host = host, Port = port, Tls = false });
            }

            IEnumerable<JToken> servershttps = jitem.SelectTokens("$.spec.servers[?(@.port.protocol == 'HTTPS')]");

            foreach (JToken item in servershttps)
            {
                var host = item.SelectToken("hosts[0]").ToString();
                var port = item.SelectToken("port.number").ToString();
                Int32.TryParse(port, out int portNumber);
                gateway.ServerPorts.Add(new ServerPort() { Host = host, Port = port, Tls = true });
            }

            // var service = jitem.SelectToken("spec.http[0].route[0].destination.host")?.ToString();
            // if (service != null)
            // {
            //     gateway.Service = service;
            // }
            // else
            // {
            //     //TODO what if protocol is not http (TCP or TLS etc..) 
            // }
            // // virtualService.Service = jitem.SelectToken("spec.http[0].route[0].destination.host").ToString();
            // gateway.Port = jitem.SelectToken("spec.http[0].route[0].destination.port.number")?.ToString();

            gateway.Name = jitem.SelectToken("metadata.name").ToString();
            gateway.Namespace = jitem.SelectToken("metadata.namespace").ToString();
            return gateway;

        }

        public static List<GatewayV1> ConvertFromJTokenToGatewayV1List(List<JToken> jitemlist)
        {
            List<GatewayV1> gatewayList = new List<GatewayV1>();
            if (jitemlist != null)
            {
                foreach (var item in jitemlist)
                {
                    if (item != null)
                    {
                        var gateway = GatewayV1.ConvertFromJTokenToGatewayV1(item);
                        gatewayList.Add(gateway);
                    }
                }
            }
            return gatewayList;
        }
    }
}