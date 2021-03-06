using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Rest;
using Microsoft.Rest.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Sentinel.K8s.DotnetKubernetesClient;

using Sentinel.K8s.DotnetKubernetesClient.Serialization;
using Sentinel.K8s.Watchers;
using Sentinel.Models.K8s.Entities;
using Sentinel.Models.K8s.LabelSelectors;

namespace Sentinel.K8s
{
#nullable enable
    public class KubernetesClient : IKubernetesClient
    {
        public IKubernetes ApiClient { get; }

        private const string DownwardApiNamespaceFile = "/var/run/secrets/kubernetes.io/serviceaccount/namespace";
        private const string DefaultNamespace = "default";
        private readonly KubernetesClientConfiguration _clientConfig;
        private readonly ILogger<KubernetesClient> _logger;

        /// <inheritdoc />
        public KubernetesClient(KubernetesClientConfiguration clientConfig, ILogger<KubernetesClient> logger)
        {
            _clientConfig = clientConfig;

            _logger = logger;

            ApiClient = new Kubernetes(clientConfig, new ClientUrlFixer())
            {
                SerializationSettings =
                {
                    Formatting = Formatting.Indented,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    ContractResolver = new KubernetesNamingConvention(),
                    Converters = new List<JsonConverter>
                    {
                        new StringEnumConverter { NamingStrategy =  new CamelCaseNamingStrategy() },
                        new Iso8601TimeSpanConverter(),
                    },
                    DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.ffffffK",
                },

                DeserializationSettings =
                {
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                    NullValueHandling = NullValueHandling.Ignore,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    ContractResolver = new KubernetesNamingConvention(),
                    Converters = new List<JsonConverter>
                    {
                        new StringEnumConverter { NamingStrategy =  new CamelCaseNamingStrategy() },
                       // new StringEnumConverter(new CamelCaseNamingStrategy()),
                        new Iso8601TimeSpanConverter(),
                    },
                    DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss.ffffffK",
                },
            };
            KubernetesClientHelper.SetTcpKeepAlives(ApiClient);

        }

        /// <inheritdoc />
        public KubernetesClient([NotNull] IKubernetes apiClient, KubernetesClientConfiguration clientConfig, ILogger<KubernetesClient> logger)
        {
            _clientConfig = clientConfig;

            _logger = logger;

            ApiClient = apiClient;
            if (apiClient is Kubernetes)
            {
                KubernetesClientHelper.SetTcpKeepAlives(ApiClient);
            }
        }

        /// <inheritdoc />
        public Task<string> GetCurrentNamespaceAsync(string downwardApiEnvName = "POD_NAMESPACE")
        {
            string result = DefaultNamespace;

            if (_clientConfig.Namespace != null)
            {
                result = _clientConfig.Namespace;
            }

            if (Environment.GetEnvironmentVariable(downwardApiEnvName) != null)
            {
                result = Environment.GetEnvironmentVariable(downwardApiEnvName) ?? string.Empty;
            }

            if (File.Exists(DownwardApiNamespaceFile))
            {
                var ns = File.ReadAllText(DownwardApiNamespaceFile);
                result = ns.Trim();
            }
            return Task.FromResult(result);
        }

        public async Task<IList<V1Namespace>> ListNamespaceAsync()
        {
            var ns = await ApiClient.ListNamespaceAsync();
            return ns.Items;
        }

        /// <inheritdoc />
        public Task<VersionInfo> GetServerVersionAsync() => ApiClient.GetCodeAsync();


        /// <inheritdoc />
        public async Task<TResource?> GetAsync<TResource>(string name, string? @namespace = null) where TResource : class, IKubernetesObject<V1ObjectMeta>
        {
            var crd = CustomEntityDefinitionExtensions.CreateResourceDefinition<TResource>();
            try
            {
                var result = await (string.IsNullOrWhiteSpace(@namespace)
                    ? ApiClient.GetClusterCustomObjectAsync(crd.Group, crd.Version, crd.Plural, name)
                    : ApiClient.GetNamespacedCustomObjectAsync(crd.Group, crd.Version, @namespace, crd.Plural, name)) as JObject;
                return result?.ToObject<TResource>();
            }
            catch (HttpOperationException e) when (e.Response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        /// <inheritdoc />
        public async Task<IList<TResource>> ListAsync<TResource>(string? @namespace = null, string? labelSelector = null) where TResource : IKubernetesObject<V1ObjectMeta>
        {
            var crd = CustomEntityDefinitionExtensions.CreateResourceDefinition<TResource>();
            var result = await (string.IsNullOrWhiteSpace(@namespace)
                ? ApiClient.ListClusterCustomObjectAsync(crd.Group, crd.Version, crd.Plural, labelSelector: labelSelector)
                : ApiClient.ListNamespacedCustomObjectAsync(crd.Group, crd.Version, @namespace, crd.Plural, labelSelector: labelSelector)) as JObject;

            var resources = result?.ToObject<EntityList<TResource>>();
            if (resources == null)
            {
                _logger.LogCritical("resources is null");
                throw new ArgumentException("Could not parse result");
            }

            return resources.Items;
        }

        public async Task<List<JToken>> ListClusterCustomObjectAsync(string Group, string Version, string Plural)
        {
            var result = await ApiClient.ListClusterCustomObjectAsync(Group, Version, Plural) as JObject;
            var res = result?.SelectToken("items")?.ToList();
            if (res == null)
            {
                res = new List<JToken>();
            }
            return res;
        }

        /// <inheritdoc />
        public Task<IList<TResource>> ListAsync<TResource>(string? @namespace = null, params ILabelSelector[] labelSelectors)
            where TResource : IKubernetesObject<V1ObjectMeta> =>
            ListAsync<TResource>(@namespace, string.Join(",", labelSelectors.Select(l => l.ToExpression())));


        /// <inheritdoc />
        public async Task<TResource> SaveAsync<TResource>(TResource resource) where TResource : class, IKubernetesObject<V1ObjectMeta>
        {
            var serverResource = await GetAsync<TResource>(resource.Metadata.Name, resource.Metadata.NamespaceProperty);
            if (serverResource == null)
            {
                return await CreateAsync(resource);
            }

            resource.Metadata.Uid = serverResource.Metadata.Uid;
            resource.Metadata.ResourceVersion = serverResource.Metadata.ResourceVersion;

            return await UpdateAsync(resource);
        }

        /// <inheritdoc />
        public async Task<TResource> CreateAsync<TResource>(TResource resource) where TResource : IKubernetesObject<V1ObjectMeta>
        {
            var crd = resource.CreateResourceDefinition();
            var result = await (string.IsNullOrWhiteSpace(resource.Metadata.NamespaceProperty)
                ? ApiClient.CreateClusterCustomObjectAsync(resource, crd.Group, crd.Version, crd.Plural)
                : ApiClient.CreateNamespacedCustomObjectAsync(resource, crd.Group, crd.Version, resource.Metadata.NamespaceProperty, crd.Plural)) as JObject;

            if (result?.ToObject(resource.GetType()) is TResource parsed)
            {
                resource.Metadata.ResourceVersion = parsed.Metadata.ResourceVersion;
                return parsed;
            }
            throw new ArgumentException("Could not parse result");
        }

        /// <inheritdoc />
        public async Task<TResource> UpdateAsync<TResource>(TResource resource) where TResource : IKubernetesObject<V1ObjectMeta>
        {
            var crd = resource.CreateResourceDefinition();
            var result = await (string.IsNullOrWhiteSpace(resource.Metadata.NamespaceProperty)
                ? ApiClient.ReplaceClusterCustomObjectAsync(resource, crd.Group, crd.Version, crd.Plural, resource.Metadata.Name)
                : ApiClient.ReplaceNamespacedCustomObjectAsync(resource, crd.Group, crd.Version, resource.Metadata.NamespaceProperty, crd.Plural, resource.Metadata.Name)) as JObject;

            if (result?.ToObject(resource.GetType()) is TResource parsed)
            {
                resource.Metadata.ResourceVersion = parsed.Metadata.ResourceVersion;
                return parsed;
            }

            throw new ArgumentException("Could not parse result");
        }

        /// <inheritdoc />
        public async Task UpdateStatusAsync<TResource>(TResource resource) where TResource : IKubernetesObject<V1ObjectMeta> //, IStatus<object>
        {
            var crd = resource.CreateResourceDefinition();
            var result = await (string.IsNullOrWhiteSpace(resource.Metadata.NamespaceProperty)
                ? ApiClient.ReplaceClusterCustomObjectStatusAsync(resource, crd.Group, crd.Version, crd.Plural, resource.Metadata.Name)
                : ApiClient.ReplaceNamespacedCustomObjectStatusAsync(resource, crd.Group, crd.Version, resource.Metadata.NamespaceProperty, crd.Plural, resource.Metadata.Name)) as JObject;

            if (result?.ToObject(resource.GetType()) is IKubernetesObject<V1ObjectMeta> parsed)
            {
                resource.Metadata.ResourceVersion = parsed.Metadata.ResourceVersion;
                return;
            }
            throw new ArgumentException("Could not parse result");
        }

        /// <inheritdoc />
        public Task DeleteAsync<TResource>(TResource resource) where TResource : IKubernetesObject<V1ObjectMeta> =>
            DeleteAsync<TResource>(resource.Metadata.Name, resource.Metadata.NamespaceProperty);

        /// <inheritdoc />
        public Task DeleteAsync<TResource>(IEnumerable<TResource> resources)
            where TResource : IKubernetesObject<V1ObjectMeta> => Task.WhenAll(resources.Select(DeleteAsync));

        /// <inheritdoc />
        public Task DeleteAsync<TResource>(params TResource[] resources)
            where TResource : IKubernetesObject<V1ObjectMeta> => Task.WhenAll(resources.Select(DeleteAsync));

        /// <inheritdoc />
        public async Task DeleteAsync<TResource>(string name, string? @namespace = null) where TResource : IKubernetesObject<V1ObjectMeta>
        {
            var crd = CustomEntityDefinitionExtensions.CreateResourceDefinition<TResource>();
            try
            {
                await (string.IsNullOrWhiteSpace(@namespace)
                    ? ApiClient.DeleteClusterCustomObjectAsync(crd.Group, crd.Version, crd.Plural, name)
                    : ApiClient.DeleteNamespacedCustomObjectAsync(crd.Group, crd.Version, @namespace, crd.Plural, name));
            }
            catch (HttpOperationException e) when (e.Response.StatusCode == HttpStatusCode.NotFound)
            {

            }
        }

        /// <inheritdoc />
        public Task<Watcher<TResource>> WatchAsync<TResource>(
            TimeSpan timeout, Action<WatchEventType, TResource> onEvent, Action<Exception>? onError = null, Action? onClose = null,
            string? @namespace = null, CancellationToken cancellationToken = default, params ILabelSelector[] labelSelectors)
            where TResource : IKubernetesObject<V1ObjectMeta>
            => WatchAsync<TResource>(timeout, onEvent, onError, onClose, @namespace, string.Join(",", labelSelectors.Select(l => l.ToExpression())), cancellationToken);

        /// <inheritdoc />
        public Task<Watcher<TResource>> WatchAsync<TResource>(
            TimeSpan timeout, Action<WatchEventType, TResource> onEvent, Action<Exception>? onError = null, Action? onClose = null,
            string? @namespace = null, string? labelSelector = null, CancellationToken cancellationToken = default)
            where TResource : IKubernetesObject<V1ObjectMeta>
        {
            var crd = CustomEntityDefinitionExtensions.CreateResourceDefinition<TResource>();
            var result = string.IsNullOrWhiteSpace(@namespace)
                ? ApiClient.ListClusterCustomObjectWithHttpMessagesAsync(crd.Group, crd.Version, crd.Plural, labelSelector: labelSelector,
                 timeoutSeconds: (int)timeout.TotalSeconds, watch: true, cancellationToken: cancellationToken)
                : ApiClient.ListNamespacedCustomObjectWithHttpMessagesAsync(crd.Group, crd.Version, @namespace, crd.Plural, labelSelector: labelSelector,
                timeoutSeconds: (int)timeout.TotalSeconds, watch: true, cancellationToken: cancellationToken);

            return Task.FromResult(result.Watch(onEvent, onError, onClose));
        }

    }
}