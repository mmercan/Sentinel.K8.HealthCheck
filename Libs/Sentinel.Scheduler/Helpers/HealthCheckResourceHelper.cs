using Sentinel.Models.K8sDTOs;
using Sentinel.Redis;

namespace Sentinel.Scheduler.Helpers
{
    public static class HealthCheckResourceHelper
    {
        public static ServiceV1? FindServiceRelatedtoHealthCheckResourceV1(
           this HealthCheckResourceV1 healthCheckResource,
            IDictionary<string, ServiceV1>? ServiceDic
            )
        {
            if (!string.IsNullOrWhiteSpace(healthCheckResource?.Spec?.Service))
            {
                var serviceName = healthCheckResource.Spec.Service;
                var servicenameParts = serviceName.Split('.');

                string serviceKey = string.Empty;
                if (servicenameParts.Length > 1)
                {
                    var serviceNameWithoutNamespace = servicenameParts[0];
                    var namespaceName = servicenameParts[1];
                    serviceKey = $"{serviceNameWithoutNamespace}.{namespaceName}";

                }
                else if (servicenameParts.Length == 1)
                {
                    var serviceNameWithoutNamespace = servicenameParts[0];
                    var namespaceName = healthCheckResource.Namespace;
                    serviceKey = $"{serviceNameWithoutNamespace}.{namespaceName}";
                }

                if (serviceKey != string.Empty && ServiceDic != null && ServiceDic.ContainsKey(serviceKey))
                {
                    var service = ServiceDic[serviceKey];
                    if (service == null)
                    {
                        // throw new KeyNotFoundException($"Service with key {serviceKey} not found in Redis");
                    }
                    return service;
                }
                else
                {
                    return null;
                    //throw new KeyNotFoundException($"Service name not found in HealthCheckResource");
                }
            }
            else
            {
                throw new ArgumentNullException(nameof(healthCheckResource.Spec.Service));
            }


        }
    }
}