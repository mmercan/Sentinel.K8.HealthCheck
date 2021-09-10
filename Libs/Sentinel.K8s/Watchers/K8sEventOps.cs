using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using k8s;
using k8s.Models;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.Extensions.Logging;

namespace Sentinel.K8s.Watchers
{
    public class K8sEventOps
    {
        private readonly IKubernetesClient _client;
        private readonly ILogger _logger;

        public K8sEventOps(IKubernetesClient kubernetesClient, ILogger logger)
        {
            this._client = kubernetesClient;
            this._logger = logger;

        }


        public async Task<IList<Corev1Event>> GetAsync(string @namespace)
        {
            var events = await _client.ApiClient.ListNamespacedEventWithHttpMessagesAsync(@namespace); //ListNamespacedEventAsync(nameSpace);
            return events.Body.Items;
        }

        public async Task<IList<Corev1Event>> GetAllAsync()
        {
            var events = await _client.ApiClient.ListEventForAllNamespacesWithHttpMessagesAsync(); //ListEventForAllNamespacesAsync();
            return events.Body.Items;
        }

        public async Task<Corev1Event> FindEventAsync<TResource>(string @namespace, TResource resource, string message) where TResource : IKubernetesObject<V1ObjectMeta>
        {
            var events = await _client.ApiClient.ListNamespacedEventWithHttpMessagesAsync(@namespace);

            var res = events.Body.Items.FirstOrDefault(p => p.InvolvedObject.Uid == resource.Uid() && p.Message == message);
            return res;
        }


        public async Task<Corev1Event> FindEventAsync(string @namespace, string resourceUid, string message)
        {
            var events = await _client.List<Corev1Event>(@namespace);
            var res = events.FirstOrDefault(p => p.InvolvedObject.Uid == resourceUid && p.Message == message);
            return res;
        }


        public async Task<Corev1Event> CountUpOrCreateEvent<TResource>(string @namespace, TResource resource, string message,
            string reason = "Unhealthy",
            string type = "Warning") where TResource : IKubernetesObject<V1ObjectMeta>
        {
            Corev1Event v1event = null;
            v1event = await FindEventAsync(@namespace, resource, message);
            if (v1event != null)
            {
                var newcount = v1event.Count + 1;
                _logger.LogCritical("Patch existed Event : Counted : " + newcount.ToString());
                var name = v1event.Metadata.Name;

                var patch = new JsonPatchDocument<Corev1Event>();
                patch.Replace(e => e.Count, newcount);
                patch.Replace(e => e.LastTimestamp, DateTime.UtcNow);
                v1event = await _client.ApiClient.PatchNamespacedEventAsync(new V1Patch(patch), name, @namespace);
            }
            else
            {
                _logger.LogCritical("Create a new Event");
                v1event = await CreateNewEventAsync(
                    @namespace,
                    resource.Name(),
                    resource.Uid(),
                    resource.Namespace(),
                    resource.ApiVersion,
                    resource.ResourceVersion(),
                    message,
                    reason,
                    type,
                    resource.Name()
                    );
            }
            return v1event;
        }



        public async Task<Corev1Event> CountUpOrCreateEvent(
            string @namespace,
            string resourceName,
            string resourceUid,
            string resourceNamespace,
            string resourceApiVersion,
            string resourceResourceVersion,
            string message,
            string reason = "Unhealthy",
            string type = "Warning"
            )
        {
            Corev1Event v1event = null;
            v1event = await FindEventAsync(@namespace, resourceUid, message);
            if (v1event != null)
            {
                var newcount = v1event.Count + 1;
                _logger.LogCritical("Patch existed Event : Counted : " + newcount.ToString());
                var name = v1event.Metadata.Name;

                var patch = new JsonPatchDocument<Corev1Event>();
                patch.Replace(e => e.Count, newcount);
                patch.Replace(e => e.LastTimestamp, DateTime.UtcNow);
                v1event = await _client.ApiClient.PatchNamespacedEventAsync(new V1Patch(patch), name, @namespace);
            }
            else
            {
                _logger.LogCritical("Create a new Event");
                v1event = await CreateNewEventAsync(
                    @namespace,
                    resourceName,
                    resourceUid,
                    resourceNamespace,
                    resourceApiVersion,
                    resourceResourceVersion,
                    message,
                    reason,
                    type
                    );
            }
            return v1event;
        }


        public async Task<Corev1Event> CreateNewEventAsync(
            string @namespace,
            string resourceName,
            string resourceUid,
            string resourceNamespace,
            string resourceApiVersion,
            string resourceResourceVersion,
            string message,
            string reason,
            string type,
            string source = null
            )
        {

            V1ObjectReference refer = new V1ObjectReference();
            refer.ApiVersion = resourceApiVersion;
            refer.Kind = "Service";
            refer.Name = resourceName;
            refer.NamespaceProperty = resourceNamespace;
            refer.Uid = resourceUid;
            refer.ResourceVersion = resourceResourceVersion;
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
            newEvent.Metadata.NamespaceProperty = @namespace;

            newEvent.Metadata.Name = resourceName + Guid.NewGuid().ToString();
            if (source != null)
            {
                newEvent.Source = new V1EventSource(source);
            }


            newEvent.Reason = reason;
            newEvent.Type = type;

            var savedevent = await this._client.ApiClient.CreateNamespacedEventAsync(newEvent, @namespace);

            _logger.LogCritical("Saved UID :" + savedevent.Uid());
            return savedevent;

        }

    }
}