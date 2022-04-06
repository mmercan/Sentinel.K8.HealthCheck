using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Sentinel.K8s.Repos;
using Sentinel.K8s.Tests.Helpers;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.K8s.Tests
{
    public class ServiceV1K8sRepoTests
    {
        private readonly IMapper mapper;
        private readonly IKubernetesClient client;
        private readonly ITestOutputHelper _output;
        private ServiceV1K8sRepo serviceV1K8sRepo;
        public ServiceV1K8sRepoTests(ITestOutputHelper output)
        {

            client = KubernetesClientTestHelper.GetKubernetesClient();
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new K8SMapper());
            });
            mapper = config.CreateMapper();
            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<ServiceV1K8sRepo>();
            _output = output;

            serviceV1K8sRepo = new ServiceV1K8sRepo(client, mapper, logger);
        }

        [Fact]
        public void GetAllServicesWithDetails()
        {
            var services = serviceV1K8sRepo.GetAllServicesWithDetails();
            _output.WriteLine(services.Count().ToString());
            Assert.True(services.Count() > 0);
        }
    }
}