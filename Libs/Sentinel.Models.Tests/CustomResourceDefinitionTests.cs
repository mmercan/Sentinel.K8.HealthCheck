using Sentinel.Models.K8s;
using Sentinel.Models.K8s.Entities;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Models.Tests
{
    public class CustomResourceDefinitionTests
    {

        private readonly ITestOutputHelper _output;

        public CustomResourceDefinitionTests(ITestOutputHelper output) => _output = output;

        [Fact]
        public void CustomResourceDefinitionShouldCreate()
        {
            var crd = CustomEntityDefinitionExtensions.CreateResourceDefinition<CRDs.HealthCheckResource>();
            var crdd = CustomEntityDefinitionExtensions.CreateCustomResourceDefinition<CRDs.HealthCheckResource>("default");
        }

    }
}