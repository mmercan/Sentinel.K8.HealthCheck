using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using k8s.Models;
using Sentinel.K8s.Tests.Helpers;
using Sentinel.Models.CRDs;
using Sentinel.Models.K8sDTOs;
using Xunit;
using Xunit.Abstractions;
using static Sentinel.Models.CRDs.HealthCheckResource;

namespace Sentinel.K8s.Tests
{
    public class MapperTests
    {
        private readonly IMapper mapper;
        private readonly IKubernetesClient client;
        private readonly ITestOutputHelper _output;

        public MapperTests(ITestOutputHelper output)
        {

            client = KubernetesClientTestHelper.GetKubernetesClient();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new K8SMapper());
            });
            mapper = config.CreateMapper();
            _output = output;
        }

        [Fact]
        public void Map_ContainerPortV1()
        {
            V1ContainerPort port = new V1ContainerPort();
            var mapped = mapper.Map<ContainerPortV1>(port);
            Assert.NotNull(mapped);
        }

        [Fact]
        public void Map_ContainerV1()
        {
            V1Container port = new V1Container();
            var mapped = mapper.Map<ContainerV1>(port);

            mapped.StartupProbeV1 = "";
            mapped.ReadinessProbeV1 = "";
            mapped.LivenessProbeV1 = "";


            mapped.ConfigMapRef = "";
            mapped.SecretMapRef = "";


            Assert.NotNull(mapped);
        }

        [Fact]
        public void Map_DeploymentConditionV1()
        {
            V1DeploymentCondition port = new V1DeploymentCondition();
            var mapped = mapper.Map<DeploymentConditionV1>(port);

            Assert.NotNull(mapped);
        }

        [Fact]
        public void Map_DeploymentSpecV1()
        {
            V1DeploymentSpec port = new V1DeploymentSpec();
            var mapped = mapper.Map<DeploymentSpecV1>(port);
            Assert.NotNull(mapped);
        }

        [Fact]
        public void Map_DeploymentStatusV1()
        {
            V1DeploymentStatus port = new V1DeploymentStatus();
            var mapped = mapper.Map<DeploymentStatusV1>(port);
            Assert.NotNull(mapped);
        }

        [Fact]
        public async Task Map_DeploymentV1()
        {
            var deps = await client.ApiClient.ListNamespacedDeploymentWithHttpMessagesAsync("kube-system");
            var dep = deps.Body.Items[0];
            // V1Deployment port = new V1Deployment();
            var mapped = mapper.Map<DeploymentV1>(dep);
            mapped.SyncDate = System.DateTime.UtcNow;
            mapped.Deleted = false;

            Assert.NotNull(mapped);
            Assert.NotNull(mapped.NameandNamespace);

        }

        // [Fact]
        // public void Map_HttpHeaderV1()
        // {
        //     V1HTTPHeader port = new V1HTTPHeader();
        //     var mapped = mapper.Map<HttpHeaderV1>(port);
        //     Assert.NotNull(mapped);
        // }

        [Fact]
        public void Map_MetadataV1()
        {
            V1ObjectMeta port = new V1ObjectMeta();
            var mapped = mapper.Map<MetadataV1>(port);
            Assert.NotNull(mapped);
        }

        [Fact]
        public void Map_NamespaceV1()
        {
            V1Namespace port = new V1Namespace();
            var mapped = mapper.Map<NamespaceV1>(port);
            mapped.LatestSyncDateUTC = System.DateTime.UtcNow;
            mapped.ErrorCount = 10;
            mapped.ServiceCount = 10;
            mapped.WarningCount = 10;
            mapped.DeploymentCount = 10;

            Assert.NotNull(mapped);
        }

        [Fact]
        public void Map_OwnerReferenceV1()
        {
            V1OwnerReference port = new V1OwnerReference();
            var mapped = mapper.Map<OwnerReferenceV1>(port);
            Assert.NotNull(mapped);
        }

        [Fact]
        public void Map_PodSpecV1()
        {
            V1PodSpec port = new V1PodSpec();
            var mapped = mapper.Map<PodSpecV1>(port);
            Assert.NotNull(mapped);
        }

        [Fact]
        public void Map_PodTemplateSpecV1()
        {
            V1PodTemplateSpec port = new V1PodTemplateSpec();
            var mapped = mapper.Map<PodTemplateSpecV1>(port);
            Assert.NotNull(mapped);
        }

        [Fact]
        public void Map_PodV1()
        {
            V1Pod port = new V1Pod();
            var mapped = mapper.Map<PodV1>(port);
            Assert.NotNull(mapped);
        }

        [Fact]
        public void Map_ProbeV1()
        {
            V1Probe port = new V1Probe();
            var mapped = mapper.Map<ProbeV1>(port);
            Assert.NotNull(mapped);
        }

        [Fact]
        public async Task Map_ServiceV1()
        {

            var services = await client.ApiClient.ListNamespacedServiceWithHttpMessagesAsync("kube-system");
            var service = services.Body.Items[0];
            // V1Service port = new V1Service();
            var mapped = mapper.Map<ServiceV1>(service);

            mapped.IngressUrl = "";
            mapped.VirtualServiceUrl = "";
            mapped.LatestSyncDateUTC = System.DateTime.UtcNow;

            mapped.LivenessProbe = new ProbeV1();
            mapped.ReadinessProbe = new ProbeV1();
            mapped.StartupProbe = new ProbeV1();


            mapped.CronDescription = "";
            mapped.CronTab = "";
            mapped.CronTabException = "";

            Assert.NotNull(mapped);
        }


        [Fact]
        public void Map_HealthCheckResourceV1()
        {
            HealthCheckResource port = new HealthCheckResource();
            var mapped = mapper.Map<HealthCheckResourceV1>(port);
            Assert.NotNull(mapped);
        }



        [Fact]
        public void Map_HealthCheckResourceSpecV1()
        {
            HealthCheckResourceSpec port = new HealthCheckResourceSpec();
            var mapped = mapper.Map<HealthCheckResourceSpecV1>(port);
            Assert.NotNull(mapped);
        }


        [Fact]
        public void Map_HealthCheckResourceStatusV1()
        {
            HealthCheckResourceStatus port = new HealthCheckResourceStatus();
            var mapped = mapper.Map<HealthCheckResourceStatusV1>(port);
            Assert.NotNull(mapped);
        }



        [Fact]
        public void Map_DeploymentScalerResourceV1()
        {
            DeploymentScalerResource port = new DeploymentScalerResource();
            // port.
            port.Spec = new DeploymentScalerResource.DeploymentScalerResourceSpec();
            port.Status = new DeploymentScalerResource.DeploymentScalerResourceStatus();
            port.Metadata = new V1ObjectMeta();
            port.Metadata.Labels = new Dictionary<string, string>()
            {
                {"123","567"},
                {"456","789"}
            };
            var res = port.ToString();
            var mapped = mapper.Map<DeploymentScalerResourceV1>(port);
            Assert.NotNull(mapped);
        }


    }
}