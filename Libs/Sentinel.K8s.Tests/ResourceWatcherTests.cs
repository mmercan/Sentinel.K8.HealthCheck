using System;
using System.Reactive.Linq;
using System.Reflection;
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
            var loggger = Sentinel.Tests.Helpers.Helpers.GetLogger<ResourceWatcher<V1Pod>>();
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
            var loggger = Sentinel.Tests.Helpers.Helpers.GetLogger<ResourceWatcher<V1ConfigMap>>();
            var metrics = new ResourceWatcherMetrics<V1ConfigMap>(set);
            var watcher = new Watchers.ResourceWatcher<V1ConfigMap>(k8client, loggger, metrics, set);


            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromSeconds(20));


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
            Assert.NotNull(watcher);
        }


        [Fact]
        public async Task ResourceWatcherShouldHandleTimeouts()
        {
            OperatorSettings set = new OperatorSettings();
            set.Namespace = "default";
            set.WatcherHttpTimeout = 3;
            // set.Name
            var k8client = GetKubernetesClient();
            var loggger = Sentinel.Tests.Helpers.Helpers.GetLogger<ResourceWatcher<V1ConfigMap>>();
            var metrics = new ResourceWatcherMetrics<V1ConfigMap>(set);
            var watcher = new Watchers.ResourceWatcher<V1ConfigMap>(k8client, loggger, metrics, set);


            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(TimeSpan.FromSeconds(20));


            watcher.WatchEvents.Subscribe(onNext: (p) =>
            {
                _output.WriteLine("Subscribe timeout : " + p.Event.ToString() + " => " + p.Resource.Metadata.Name);
            }
            , onError: (ex) =>
            {
                _output.WriteLine("Subscribe timeout  Error : " + ex.Message);
            }
            // ,onCompleted:  (done)=>{
            //      _output.WriteLine("Subscribe Done");
            // }
            // ,token:source

            );


            await watcher.Start();

            await watcher.Start();

            _output.WriteLine("Subscribe ResourceWatcherShouldHandleTimeouts started");
            await Task.Delay(TimeSpan.FromSeconds(20));
            await watcher.Stop();
            watcher.Dispose();

            await Task.Delay(TimeSpan.FromSeconds(2));
            Assert.NotNull(watcher);
        }


        [Fact]
        public async Task ResourceWatcherOnCloseShouldTriggerRestart()
        {
            OperatorSettings set = new OperatorSettings();
            set.Namespace = "default";
            set.WatcherHttpTimeout = 3;
            // set.Name
            var k8client = GetKubernetesClient();
            var loggger = Sentinel.Tests.Helpers.Helpers.GetLogger<ResourceWatcher<V1ConfigMap>>();
            var metrics = new ResourceWatcherMetrics<V1ConfigMap>(set);
            var watcher = new Watchers.ResourceWatcher<V1ConfigMap>(k8client, loggger, metrics, set);


            CancellationTokenSource source = new CancellationTokenSource();
            source.CancelAfter(60 * 1000);


            watcher.WatchEvents.Subscribe(onNext: (p) =>
            {
                _output.WriteLine("Subscribe timeout : " + p.Event.ToString() + " => " + p.Resource.Metadata.Name);
            }
            , onError: (ex) =>
            {
                _output.WriteLine("Subscribe timeout  Error : " + ex.Message);
            }
            );

            await watcher.Start();

            var underlyingHandlerMethod = watcher.GetType().GetMethod("OnClose", BindingFlags.NonPublic | BindingFlags.Instance);
            underlyingHandlerMethod?.Invoke(watcher, null);


            var underlyingExceptionMethod = watcher.GetType().GetMethod("OnException", BindingFlags.NonPublic | BindingFlags.Instance);

            var expparams = new object[1] { new Exception() };
            underlyingExceptionMethod?.Invoke(watcher, expparams);


            await Task.Delay(TimeSpan.FromSeconds(2));


            await watcher.Stop();
            Assert.NotNull(watcher);
        }


    }
}