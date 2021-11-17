using k8s;
using Xunit;
using Sentinel.K8s.Tests.Helpers;
using Xunit.Abstractions;
using System.Linq;
using k8s.Models;
using System.Collections.Generic;
using System.Text;
using System;
using System.Threading.Tasks;
using System.Threading;
using Newtonsoft.Json.Linq;
using Sentinel.Models.CRDs;


namespace Sentinel.K8s.Tests
{
    public class KubernetesClientTests
    {

        private readonly ITestOutputHelper _output;

        public KubernetesClientTests(ITestOutputHelper output) => _output = output;

        // [Fact]
        // public void Should_KubernetesClient_Creates_Default_Instance()
        // {
        //     KubernetesClient client = new KubernetesClient();
        //     Assert.NotNull(client.ApiClient);
        // }

        [Fact]
        public void Should_KubernetesClient_Creates_Instance_With_clientConfig()
        {
            KubernetesClientConfiguration config = k8s.KubernetesClientConfiguration.BuildConfigFromConfigFile();
            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<KubernetesClient>();
            KubernetesClient client = new KubernetesClient(config, logger);
            Assert.NotNull(client.ApiClient);
        }


        [Fact]
        public void Should_KubernetesClient_Creates_Instance_With_clientConfig_and_Client()
        {

            KubernetesClientConfiguration config = k8s.KubernetesClientConfiguration.BuildConfigFromConfigFile();
            Kubernetes apiClient = new Kubernetes(config);
            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<KubernetesClient>();
            KubernetesClient client = new KubernetesClient(apiClient, config, logger);
            Assert.NotNull(client.ApiClient);
        }

        [Fact]
        public void Should_KubernetesClient_Returns_List_From_GetCurrentNamespace()
        {

            var k8Client = KubernetesClientTestHelper.GetKubernetesClient();
            var nssTask = k8Client.GetCurrentNamespace();
            nssTask.Wait();
            _output.WriteLine("GetCurrentNamespace : " + nssTask.Result);
            Assert.NotNull(nssTask.Result);
        }


        [Fact]
        public void Should_KubernetesClient_Returns_Server_Version()
        {

            var k8Client = KubernetesClientTestHelper.GetKubernetesClient();
            var nssTask = k8Client.GetServerVersion();
            nssTask.Wait();
            _output.WriteLine("Server Version : " + nssTask.Result.Major + "." + nssTask.Result.Minor);
            Assert.NotNull(nssTask.Result);
        }



        [Theory]
        [InlineData("kubernetes-dashboard", "kube-system")]
        public void Should_KubernetesClient_Returns_A_Resource_From_Get(string serviceName, string @namespace)
        {
            var k8Client = KubernetesClientTestHelper.GetKubernetesClient();
            var nssTask = k8Client.Get<k8s.Models.V1Service>(name: serviceName, @namespace: @namespace);
            nssTask.Wait();
            _output.WriteLine("Get a Single Resource : " + nssTask?.Result?.Metadata.Name);
            Assert.NotNull(nssTask?.Result);
        }


        [Fact]
        public void Should_KubernetesClient_Returns_ListCluster()
        {

            var k8Client = KubernetesClientTestHelper.GetKubernetesClient();
            var nssTask = k8Client.List<k8s.Models.V1Namespace>();
            nssTask.Wait();
            string namespaces = string.Join(",", nssTask.Result.Select(p => p.Metadata.Name));
            _output.WriteLine("namespaces : " + namespaces);

            // Assert.Collection<k8s.Models.V1Namespace>(nssTask.Result, item => Assert.NotEmpty(item.Metadata.Name));
            Assert.NotNull(nssTask.Result);
        }


        [Fact]
        public void Should_KubernetesClient_Returns_NamespacedResource()
        {

            var k8Client = KubernetesClientTestHelper.GetKubernetesClient();
            var nssTask = k8Client.List<k8s.Models.V1Service>("kube-system");
            nssTask.Wait();
            string namespaces = string.Join(",", nssTask.Result.Select(p => p.Metadata.Name));
            _output.WriteLine("services : " + namespaces);

            // Assert.Collection<k8s.Models.V1Namespace>(nssTask.Result, item => Assert.NotEmpty(item.Metadata.Name));
            Assert.NotNull(nssTask.Result);
        }


        [Fact]
        public void Should_KubernetesClient_Save_NewResource()
        {

            k8s.Models.V1ConfigMap config = new k8s.Models.V1ConfigMap();

            var dateann = DateTime.UtcNow.ToString();
            var metadata = new V1ObjectMeta(
                    annotations: new Dictionary<string, string> { { "app", "test" }, { "person", "chummy" }, { "updated", dateann } },
                    labels: new Dictionary<string, string> { { "app", "test" }, { "person", "chummy" } },
                    name: "testconfigmap",
                    namespaceProperty: "default"
                );
            config.Metadata = metadata;
            config.Immutable = false;

            config.Data = new Dictionary<string, string> { { "appsettingkey1", "config-value" }, { "appsettingkey2", "config-value" }, { "updated", dateann } };
            // config.BinaryData = new Dictionary<string, byte[]> { { "signature", Encoding.ASCII.GetBytes("config text") }};

            var k8Client = KubernetesClientTestHelper.GetKubernetesClient();
            var nssTask = k8Client.Save<V1ConfigMap>(config);
            nssTask.Wait();
            // string resource = string.Join(",", nssTask.Result.Select(p => p.Metadata.Name));
            _output.WriteLine("Saved ConfigMap : " + nssTask.Result.Name());

            Assert.NotNull(nssTask.Result);
        }

        [Fact]
        public async Task Should_KubernetesClient_Save_ExistedResource()
        {

            Should_KubernetesClient_Save_NewResource();
            await Task.Delay(3000);
            Should_KubernetesClient_Save_NewResource();
            await Task.Delay(3000);
            Should_KubernetesClient_Delete_ExistedResource();
        }

        [Fact]
        public void Should_KubernetesClient_Delete_ExistedResource()
        {

            Should_KubernetesClient_Save_NewResource();
            Task.Delay(3000);
            var k8Client = KubernetesClientTestHelper.GetKubernetesClient();

            var nssTask = k8Client.Get<k8s.Models.V1ConfigMap>(name: "testconfigmap", @namespace: "default");
            nssTask.Wait();
            _output.WriteLine("Get a Single Resource : " + nssTask.Result?.Metadata.Name);
            if (nssTask.Result != null)
            {
                var deletetask = k8Client.Delete(nssTask.Result);
                deletetask.Wait();
            }
            else
            {
                _output.WriteLine("KubernetesClientTests Should_KubernetesClient_Delete_ExistedResource V1ConfigMap Null");
            }


        }

        [Fact]
        public async Task Should_KubernetesClient_Retreive_Wait()
        {

            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(60 * 1000);
            //var taskwatch = watchClient.StartAsync(source.Token);

            var k8Client = KubernetesClientTestHelper.GetKubernetesClient();
            await k8Client.Watch<k8s.Models.V1ConfigMap>(TimeSpan.FromMinutes(2),
             onEvent: (action, p) =>
             {
                 _output.WriteLine("Watch has new event " + action.ToString() + " : " + p.Metadata.Name);
                 if (action == WatchEventType.Added)
                 {

                 }
             }, onError: (ex) =>
             {
                 _output.WriteLine("Watch has Error " + ex.Message);
             }, onClose: () =>
             {
                 _output.WriteLine("Watch Closed ");
             }, cancellationToken: source.Token);

        }


        [Theory]
        [InlineData("my-new-healthcheck-object-2", "default")]
        public async Task Should_KubernetesClient_UpdateStatus(string name, string @namespace)
        {

            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(60 * 1000);

            var k8Client = KubernetesClientTestHelper.GetKubernetesClient();
            var statusresobj = await k8Client.Get<HealthCheckResource>(name: name, @namespace: @namespace);
            if (statusresobj != null)
            {


                if (statusresobj.Status == null)
                {
                    statusresobj.Status = new HealthCheckResource.HealthCheckResourceStatus();
                    _output.WriteLine("No Status Found ");
                }
                else
                {
                    _output.WriteLine("Status Found " + statusresobj.Status.Phase);
                }

                statusresobj.Status.Phase = "Starting";
                statusresobj.Status.LastCheckTime = DateTime.Now.ToString();
                statusresobj.Status.Message = "Message added";

                _output.WriteLine("Status will be saved " + statusresobj.ToString() + " " + statusresobj.Status.Phase);
                await k8Client.UpdateStatus<HealthCheckResource>(statusresobj);
            }
            else
            {
                _output.WriteLine("Should_KubernetesClient_UpdateStatus statusresobj is NULL");
            }
        }

        [Fact]
        public async Task Should_KubernetesClient_Returns_List_of_Custom_Resources()
        {
            var k8Client = KubernetesClientTestHelper.GetKubernetesClient();
            var healthchecks = await k8Client.List<HealthCheckResource>(@namespace: "default");
            _output.WriteLine("healthchecks listed");
            foreach (var item in healthchecks)
            {
                _output.WriteLine("hc : " + item.ToString());
            }

        }



    }
}