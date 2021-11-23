using System;
using System.Threading.Tasks;
using Sentinel.K8s.Tests.Helpers;
using Sentinel.K8s.Watchers;
using Sentinel.Models.CRDs;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.K8s.Tests
{
    public class K8SEventOpsTests
    {

        private readonly ITestOutputHelper _output;

        public K8SEventOpsTests(ITestOutputHelper output) => _output = output;


        [Theory]
        [InlineData("my-new-healthcheck-object-2", "default", "message_")]
        public async Task K8SEventOpsShuldHaveAnInstance(string name, string @namespace, string message)
        {

            var message_1 = message + DateTime.Now.ToString();
            var k8Client = KubernetesClientTestHelper.GetKubernetesClient();
            var healthobj = await k8Client.Get<HealthCheckResource>(name: name, @namespace: @namespace);

            if (healthobj != null)
            {
                K8SEventOps ops = KubernetesClientTestHelper.GetK8SEventOps();

                //Create new Event
                var eve = await ops.CountUpOrCreateEvent<HealthCheckResource>(@namespace, healthobj, message_1);

                //Count up 
                var eve_2 = await ops.CountUpOrCreateEvent<HealthCheckResource>(@namespace, healthobj, message_1);
                _output.WriteLine("event message : " + eve_2.Message);
                Assert.NotNull(eve_2.Metadata.Name);
            }
            else
            {
                _output.WriteLine("healthobj is Null");
            }

        }


        [Theory]
        [InlineData("default")]
        public async Task K8SEventOpsListEventsInNamespace(string @namespace)
        {
            K8SEventOps ops = KubernetesClientTestHelper.GetK8SEventOps();

            var eventList = await ops.GetAsync(@namespace);

            _output.WriteLine("event count : " + eventList.Count);
            Assert.NotNull(eventList);

        }

        [Fact]
        public async Task K8SEventOpsListEventsInAllNamespaces()
        {
            K8SEventOps ops = KubernetesClientTestHelper.GetK8SEventOps();

            var eventList = await ops.GetAllAsync();

            _output.WriteLine("event count : " + eventList.Count);
            Assert.NotNull(eventList);

        }
    }
}