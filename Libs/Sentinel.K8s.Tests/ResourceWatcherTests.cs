using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using k8s.Models;
using Sentinel.K8s.Watchers;
using Xunit;
using Xunit.Abstractions;
using static Sentinel.K8s.Tests.Helpers.KubernetesClientTestHelper;

namespace Sentinel.K8s.Tests
{
    public class ResourceWatcherTests
    {
        private readonly ITestOutputHelper _output;
        public ResourceWatcherTests(ITestOutputHelper output) => _output = output;


        [Fact]
        public void ResourceWatcherShouldHaveInstance()
        {
            OperatorSettings set = new OperatorSettings();
            // set.Name
            var k8client = GetKubernetesClient();
            var loggger = GetLogger<ResourceWatcher<V1Pod>>();
            var metrics = new ResourceWatcherMetrics<V1Pod>(set);
            var watcher = new Watchers.ResourceWatcher<V1Pod>(k8client, loggger, metrics, set);

            Assert.NotNull(watcher);
        }



        [Fact]
        public async Task ResourceWatcherShouldHaveWatch()
        {
            OperatorSettings set = new OperatorSettings();
            set.Namespace = "default";
            // set.Name
            var k8client = GetKubernetesClient();
            var loggger = GetLogger<ResourceWatcher<V1ConfigMap>>();
            var metrics = new ResourceWatcherMetrics<V1ConfigMap>(set);
            var watcher = new Watchers.ResourceWatcher<V1ConfigMap>(k8client, loggger, metrics, set);


            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(60 * 1000);


            watcher.WatchEvents.Subscribe(onNext: (p) =>
            {

                _output.WriteLine("Subscribe : " + p.Event.ToString() + " => " + p.Resource.Metadata.Name);
            }
            , onError: (ex) =>
            {
                _output.WriteLine("Subscribe Error : " + ex.Message);
            }
            // ,onCompleted:  (done)=>{
            //      _output.WriteLine("Subscribe Done");
            // }
            // ,token:source

            );


            await watcher.Start();
            _output.WriteLine("Subscribe ResourceWatcherShouldHaveWatch started");
            await Task.Delay(TimeSpan.FromSeconds(20));
            await watcher.Stop();
        }



    }
}