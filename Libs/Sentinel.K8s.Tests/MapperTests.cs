using AutoMapper;
using k8s.Models;
using Sentinel.Models.K8sDTOs;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.K8s.Tests
{
    public class MapperTests
    {
        private readonly IMapper mapper;
        private readonly ITestOutputHelper _output;

        public MapperTests(ITestOutputHelper output)
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new K8SMapper());
            });
            mapper = config.CreateMapper();
            _output = output;
        }

        [Fact]
        public void MapContainerPortV1()
        {
            V1ContainerPort port = new V1ContainerPort();
            var mapped = mapper.Map<ContainerPortV1>(port);
            Assert.NotNull(mapped);
        }


        public void MapContainerV1()
        {
            V1Container port = new V1Container();
            var mapped = mapper.Map<ContainerV1>(port);
            Assert.NotNull(mapped);
        }
        public void MapDeploymentConditionV1()
        {
            V1DeploymentCondition port = new V1DeploymentCondition();
            var mapped = mapper.Map<DeploymentConditionV1>(port);
            Assert.NotNull(mapped);
        }
        public void MapDeploymentSpecV1()
        {
            V1DeploymentSpec port = new V1DeploymentSpec();
            var mapped = mapper.Map<DeploymentSpecV1>(port);
            Assert.NotNull(mapped);
        }
        public void MapDeploymentStatusV1()
        {
            V1DeploymentStatus port = new V1DeploymentStatus();
            var mapped = mapper.Map<DeploymentStatusV1>(port);
            Assert.NotNull(mapped);
        }
        public void MapDeploymentV1()
        {
            V1Deployment port = new V1Deployment();
            var mapped = mapper.Map<DeploymentV1>(port);
            Assert.NotNull(mapped);
        }
        public void MapHttpHeaderV1()
        {
            V1HTTPHeader port = new V1HTTPHeader();
            var mapped = mapper.Map<HttpHeaderV1>(port);
            Assert.NotNull(mapped);
        }

        public void MapMetadataV1()
        {
            V1ObjectMeta port = new V1ObjectMeta();
            var mapped = mapper.Map<MetadataV1>(port);
            Assert.NotNull(mapped);
        }
        public void MapNamespaceV1()
        {
            V1Namespace port = new V1Namespace();
            var mapped = mapper.Map<NamespaceV1>(port);
            Assert.NotNull(mapped);
        }
        public void MapOwnerReferenceV1()
        {
            V1OwnerReference port = new V1OwnerReference();
            var mapped = mapper.Map<OwnerReferenceV1>(port);
            Assert.NotNull(mapped);
        }
        public void MapPodSpecV1()
        {
            V1PodSpec port = new V1PodSpec();
            var mapped = mapper.Map<PodSpecV1>(port);
            Assert.NotNull(mapped);
        }
        public void MapPodTemplateSpecV1()
        {
            V1PodTemplateSpec port = new V1PodTemplateSpec();
            var mapped = mapper.Map<PodTemplateSpecV1>(port);
            Assert.NotNull(mapped);
        }
        public void MapPodV1()
        {
            V1Pod port = new V1Pod();
            var mapped = mapper.Map<PodV1>(port);
            Assert.NotNull(mapped);
        }
        public void MapProbeV1()
        {
            V1Probe port = new V1Probe();
            var mapped = mapper.Map<ProbeV1>(port);
            Assert.NotNull(mapped);
        }
        public void MapServiceV1()
        {
            V1Service port = new V1Service();
            var mapped = mapper.Map<ServiceV1>(port);
            Assert.NotNull(mapped);
        }

    }
}