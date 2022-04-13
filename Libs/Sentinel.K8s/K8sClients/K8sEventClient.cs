using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using k8s;
using k8s.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;

namespace Sentinel.K8s.K8sClients
{
    public class K8sEventClient
    {
        private readonly IKubernetesClient _k8sclient;
        private readonly IMapper _mapper;
        private readonly ILogger<K8sGeneralService> _logger;

        public K8sEventClient(IKubernetesClient k8sclient, IMapper mapper, ILogger<K8sGeneralService> logger)
        {
            _k8sclient = k8sclient;
            _mapper = mapper;
            _logger = logger;
            // V1Service
        }


        public IList<Corev1Event> Get(string nameSpace)
        {
            var events = _k8sclient.ApiClient.ListNamespacedEvent(nameSpace);
            return events.Items;

        }

        public async Task<IList<Corev1Event>> GetAsync(string nameSpace)
        {
            var events = await _k8sclient.ApiClient.ListNamespacedEventAsync(nameSpace);
            return events.Items;
        }

        public async Task<IList<Corev1Event>> GetAllAsync()
        {
            var events = await _k8sclient.ApiClient.ListEventForAllNamespacesAsync();
            return events.Items;
        }

        public async Task<Corev1Event> FindEventAsync(string namespaceParam, string Uid, string message)
        {
            var events = await _k8sclient.ApiClient.ListNamespacedEventAsync(namespaceParam);
            var res = events.Items.FirstOrDefault(p => p.InvolvedObject.Uid == Uid && p.Message == message);
            return res;
        }


        // public async Task<Corev1Event> FindEventAsync(string namespaceParam, string serviceUid, string message)
        // {
        //     var events = await _k8sclient.ApiClient.ListNamespacedEventAsync(namespaceParam);

        //     var res = events.Items.FirstOrDefault(p => p.InvolvedObject.Uid == serviceUid && p.Message == message);
        //     return res;
        // }


        // public async Task<Corev1Event> CountUpOrCreateEvent(string namespaceParam, V1Service service, string message,
        //     string reason = "Unhealthy",
        //     string type = "Warning")
        // {
        //     Corev1Event v1event = null;
        //     v1event = await FindEventAsync(namespaceParam, service.Uid(), message);
        //     if (v1event != null)
        //     {
        //         var newcount = v1event.Count + 1;
        //         _logger.LogCritical("Patch existed Event : Counted : " + newcount.ToString());
        //         var name = v1event.Metadata.Name;

        //         var patch = new JsonPatchDocument<Corev1Event>();
        //         patch.Replace(e => e.Count, newcount);
        //         patch.Replace(e => e.LastTimestamp, DateTime.UtcNow);
        //         v1event = await _k8sclient.ApiClient.PatchNamespacedEventAsync(new V1Patch(patch), name, namespaceParam);
        //     }
        //     else
        //     {
        //         _logger.LogCritical("Create a new Event");
        //         v1event = await CreateNewEventAsync(
        //             namespaceParam,
        //             service.Name(),
        //             service.Uid(),
        //             service.Namespace(),
        //             service.ApiVersion,
        //             service.ResourceVersion(),
        //             message,
        //             reason,
        //             type
        //             );
        //     }
        //     return v1event;
        // }



        public async Task<Corev1Event> CountUpOrCreateEvent(
            string Namespace,
            string Name,
            string Uid,
            //string serviceNamespace,
            string ApiVersion,
            string ResourceVersion,
            string message,
            string kind,
            string reason = "Unhealthy",
            string type = "Warning"
            )
        {
            Corev1Event v1event = null;
            v1event = await FindEventAsync(Namespace, Uid, message);
            if (v1event != null)
            {
                var newcount = v1event.Count + 1;
                _logger.LogCritical("Patch existed Event : Counted : " + newcount.ToString());
                var name = v1event.Metadata.Name;

                var patch = new JsonPatchDocument<Corev1Event>();
                patch.Replace(e => e.Count, newcount);
                patch.Replace(e => e.LastTimestamp, DateTime.UtcNow);
                v1event = await _k8sclient.ApiClient.PatchNamespacedEventAsync(new V1Patch(patch), name, Namespace);
            }
            else
            {
                _logger.LogCritical("Create a new Event");
                v1event = await CreateNewEventAsync(
                    Namespace,
                    Name,
                    Uid,
                    ApiVersion,
                    ResourceVersion,
                    message,
                    reason,
                    type,
                    kind
                    );
            }
            return v1event;
        }


        public async Task<Corev1Event> CreateNewEventAsync(
            string Namespace,
            string name,
            string uid,
            string apiVersion,
            string resourceVersion,
            string message,
            string reason,
            string type,
            string kind
            )
        {

            V1ObjectReference refer = new V1ObjectReference();
            refer.ApiVersion = apiVersion;
            refer.Kind = kind;
            refer.Name = name;
            refer.NamespaceProperty = Namespace;
            refer.Uid = uid;
            refer.ResourceVersion = resourceVersion;
            refer.FieldPath = "metadata.annotations";

            Corev1Event newEvent = new Corev1Event { };
            newEvent.FirstTimestamp = DateTime.UtcNow;
            newEvent.LastTimestamp = DateTime.UtcNow;
            newEvent.InvolvedObject = refer;
            newEvent.Count = 1;
            newEvent.LastTimestamp = DateTime.UtcNow;
            newEvent.Message = message;
            newEvent.Metadata = new V1ObjectMeta();
            newEvent.Metadata.CreationTimestamp = DateTime.UtcNow;
            newEvent.Metadata.NamespaceProperty = Namespace;

            newEvent.Metadata.Name = name + Guid.NewGuid().ToString();

            newEvent.Reason = reason;
            newEvent.Type = type;

            var savedevent = await this._k8sclient.ApiClient.CreateNamespacedEventAsync(newEvent, Namespace);

            _logger.LogCritical("Saved UID :" + savedevent.Uid());
            return savedevent;

        }
    }
}