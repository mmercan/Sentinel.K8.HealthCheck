using System;
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
        [InlineData("my-new-healthcheck-object-2", "default", "message_")]
        public async Task K8sEventOpsShuldHaveAnInstance(string name, string @namespace, string message)
        {

            var message_1 = message + DateTime.Now.ToString();
            var k8Client = KubernetesClientTestHelper.GetKubernetesClient();
            var healthobj = await k8Client.Get<HealthCheckResource>(name: name, @namespace: @namespace);

            var hel = healthobj.ToString();

            K8sEventOps ops = KubernetesClientTestHelper.GetK8sEventOps();


            //Create new Event
            var eve = await ops.CountUpOrCreateEvent<HealthCheckResource>(@namespace, healthobj, message);


            //Count up 
            var eve_2 = await ops.CountUpOrCreateEvent<HealthCheckResource>(@namespace, healthobj, message);
            _output.WriteLine("event message : " + eve_2.Message);
            Assert.NotNull(eve_2.Metadata.Name);

        }


        [Theory]
        [InlineData("default")]
        public async Task K8sEventOpsListEventsInNamespace(string @namespace)
        {
            K8sEventOps ops = KubernetesClientTestHelper.GetK8sEventOps();

            var eventList = await ops.GetAsync(@namespace);

            _output.WriteLine("event count : " + eventList.Count);
            Assert.NotNull(eventList);

        }

        [Fact]
        public async Task K8sEventOpsListEventsInAllNamespaces()
        {
            K8sEventOps ops = KubernetesClientTestHelper.GetK8sEventOps();

            var eventList = await ops.GetAllAsync();

            _output.WriteLine("event count : " + eventList.Count);
            Assert.NotNull(eventList);

        }
    }
}