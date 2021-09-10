using System.Threading.Tasks;
using Sentinel.K8s.Tests.Helpers;
using Sentinel.K8s.Watchers;
using Sentinel.Models.CRDs;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.K8s.Tests
{
    public class K8sEventOpsTests
    {

        private readonly ITestOutputHelper _output;

        public K8sEventOpsTests(ITestOutputHelper output) => _output = output;


        [Theory]
        [InlineData("my-new-healthcheck-object-2", "default", "message_2")]
        public async Task K8sEventOpsShuldHaveAnInstance(string name, string @namespace, string message)
        {

            var k8Client = KubernetesClientTestHelper.GetKubernetesClient();
            var healthobj = await k8Client.Get<HealthCheckResource>(name: name, @namespace: @namespace);

            var hel = healthobj.ToString();

            K8sEventOps ops = KubernetesClientTestHelper.GetK8sEventOps();

            var eve = await ops.CountUpOrCreateEvent<HealthCheckResource>(@namespace, healthobj, message);

            _output.WriteLine("event message : " + eve.Message);
            Assert.NotNull(eve.Metadata.Name);



        }
    }
}