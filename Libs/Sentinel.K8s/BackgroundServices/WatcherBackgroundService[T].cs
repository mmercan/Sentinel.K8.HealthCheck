
using k8s;
using k8s.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Sentinel.Common;
using Sentinel.K8s.Watchers;

namespace Sentinel.K8s.BackgroundServices
{
    public abstract class WatcherBackgroundService<TEntity> : BackgroundServiceWithHealthCheck
    where TEntity : IKubernetesObject<V1ObjectMeta>
    {
        protected readonly IConfiguration _configuration;
        private readonly IKubernetesClient _client;
        protected string Name { get; set; } = default!;
        protected string? Namespace { get; set; } = default!;
        public ResourceWatcher<TEntity> Watcher { get; set; } = default!;
        public WatcherBackgroundService(IConfiguration configuration, IKubernetesClient client,
            ILogger<WatcherBackgroundService<TEntity>> logger, IOptions<HealthCheckServiceOptions> hcoptions) : base(logger, hcoptions)
        {
            _configuration = configuration;
            _client = client;

            getAttributeDetails();

            var settings = Options.Create(new OperatorSettings<TEntity>());
            settings.Value.Namespace = Namespace;
            settings.Value.Name = Name;
            settings.Value.WatcherHttpTimeout = 100;
            ResourceWatcherMetrics<TEntity> metrics = new ResourceWatcherMetrics<TEntity>(settings);

            Watcher = new ResourceWatcher<TEntity>(client, logger, metrics, settings);
            executingTask = Task.CompletedTask;
        }

        public abstract void Watch(WatchEventType Event, TEntity Resource);

        protected override Task Execute(CancellationToken stoppingToken)
        {
            Watcher.Start();
            Watcher.WatchEvents.Subscribe(x =>
                        {
                            Watch(x.Event, x.Resource);
                        });
            return Task.CompletedTask;
        }


        private void getAttributeDetails()
        {
            Type t = this.GetType();
            // Get instance of the attribute.
            K8sWatcherAttribute watcherAttribute = (K8sWatcherAttribute)Attribute.GetCustomAttribute(t, typeof(K8sWatcherAttribute));
            if (watcherAttribute == null)
            {
                _logger.LogWarning("K8sWatcherAttribute The attribute was not found. on {appName}", appName);
            }
            else
            {
                var AppName = t.Name;
                Name = string.IsNullOrEmpty(watcherAttribute.Name) ? t.Name : watcherAttribute.Name;
                if (watcherAttribute.TimeoutTotalMinutes > 0)
                {
                    timeout = TimeSpan.FromMinutes(watcherAttribute.TimeoutTotalMinutes);
                }
                _logger.LogInformation("The Name Attribute is: {0}. Namespace : {Namespace}, Watch All Namespaces {WatchAll} Enabled : {Enabled}",
                 watcherAttribute.Name, watcherAttribute.Namespace, watcherAttribute.WatchAllNamespaces, watcherAttribute.Enabled);
                _logger.LogInformation("The Description Attribute is: {0}.", watcherAttribute.Description);
                if (watcherAttribute.WatchAllNamespaces)
                {
                    Namespace = null;
                }
                else
                {
                    Namespace = watcherAttribute.Namespace;
                }

            }
        }
    }
}






// using System;
// using System.Collections.Generic;
// using System.Collections.ObjectModel;
// using System.Linq;
// using System.Threading.Tasks;
// using AutoMapper;
// using k8s;
// using k8s.Models;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Diagnostics.HealthChecks;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
// using Sentinel.Common;
// using Sentinel.K8s.Watchers;

// namespace Sentinel.K8s.BackgroundServices
// {
//     public abstract class WatcherBackgroundService<T> : BackgroundService where T : IKubernetesObject<V1ObjectMeta>
//     {
//         // private readonly IWatcher<T> _watcher;
//         // private readonly IEnumerable<IWatcherHandler<T>> _handlers;

//         // public WatcherBackgroundService(IWatcher<T> watcher, IEnumerable<IWatcherHandler<T>> handlers)
//         // {
//         //     _watcher = watcher;
//         //     _handlers = handlers;
//         // }


//         protected DateTime lastrestart = DateTime.UtcNow;
//         protected Task executingTask;
//         protected readonly IKubernetesClient _k8sService;
//         protected readonly IMapper _mapper;
//         protected readonly IConfiguration _configuration;
//         protected readonly ILogger<WatcherBackgroundService<T>> _logger;
//         protected readonly BackgroundServiceHealthCheck bgHealthCheck = default!;
//         protected TimeSpan? timeout;
//         protected DateTime LastRun { get; set; } = DateTime.UtcNow;
//         private Timer _timer;
//         protected bool isTriggered { get; set; } = true;
//         protected ManualResetEventSlim _ResetEvent = new ManualResetEventSlim(false);

//         public ObservableCollection<T> _items;

//         private CancellationToken cancellationToken;
//         private ResourceWatcher<T> _watcher;

//         protected virtual string appName
//         {
//             get { return this.GetType().Name; }
//         }



//         public WatcherBackgroundService(
//             ResourceWatcher<T> watcher,
//               ILogger<WatcherBackgroundService<T>> logger,
//                     IKubernetesClient k8sService,
//                     IConfiguration configuration,
//                     IMapper mapper,
//                     ObservableCollection<T> items,
//                     IOptions<HealthCheckServiceOptions> hcoptions)
//         {
//             _k8sService = k8sService;
//             _configuration = configuration;
//             _logger = logger;
//             _mapper = mapper;
//             _items = items;
//             if (hcoptions.Value != null)
//             {
//                 var name = this.GetType().ToString();
//                 bgHealthCheck = new BackgroundServiceHealthCheck();
//                 var registration = new HealthCheckRegistration(name, bgHealthCheck, null, null);
//                 hcoptions.Value.Registrations.Add(registration);
//                 ReportHealthy(name + " initialized");
//             }

//             _watcher = watcher;
//             getAttributeDetails();
//             executingTask = Task.CompletedTask;
//         }


//         protected override Task ExecuteAsync(CancellationToken stoppingToken)
//         {
//             cancellationToken = stoppingToken;
//             executingTask = Task.Factory.StartNew(() => WatchStarter(stoppingToken), TaskCreationOptions.LongRunning);
//             if (executingTask.IsCompleted)
//             {
//                 return executingTask;
//             }
//             return Task.CompletedTask;

//             _watcher.Start();
//             _watcher.WatchEvents.Subscribe(
//                 (e) =>
//                 {
//                     if (e.Event == WatchEventType.Added)
//                     {
//                         _items.Add(_mapper.Map<T>(e.Resource));
//                     }
//                     else if (e.Event == WatchEventType.Deleted)
//                     {
//                         var item = _items.FirstOrDefault(x => x.Metadata.Name == e.Resource.Metadata.Name);
//                         if (item != null)
//                         {
//                             _items.Remove(item);
//                         }
//                     }
//                     else if (e.Event == WatchEventType.Modified)
//                     {
//                         var item = _items.FirstOrDefault(x => x.Metadata.Name == e.Resource.Metadata.Name);
//                         if (item != null)
//                         {
//                             _items.Remove(item);
//                             _items.Add(_mapper.Map<T>(e.Resource));
//                         }
//                     }
//                 },
//                 (e) =>
//                 {
//                     _logger.LogError(e, "Error in watcher");
//                 },
//                 () =>
//                 {
//                     _logger.LogInformation("Watcher completed");
//                 });
//         }



//         private void WatchStarter(CancellationToken stoppingToken)
//         {
//             this.ReportHealthy();
//             this._logger.LogCritical("Watch Started");

//             using (_k8sService.WatchAsync<T>(timeout: TimeSpan.FromMinutes(60),
//                 onEvent: watcherInternal,
//                 onError: OnError,
//                 onClose: OnClosed,
//                 cancellationToken: stoppingToken))
//             {
//                 this._logger.LogCritical("=== on watch Done ===");
//                 var ctrlc = new ManualResetEventSlim(false);
//                 ctrlc.Wait(CancellationToken.None);
//             }
//         }

//         private void watcherInternal(WatchEventType type, T item)
//         {
//             var receivedType = item.GetType().Name;
//             _logger.LogInformation("{appName}: {type} received {receivedType} OnEvent: {name}", appName, type, receivedType, item.Name());
//             this.ReportHealthy("received new " + receivedType + " " + item.Name());
//             watcher(type, item).Wait();

//             if (type == WatchEventType.Added)
//             {
//                 _items.Add(item);
//             }
//             else if (type == WatchEventType.Modified)
//             {
//                 var itemInList = _items.FirstOrDefault(x => x.Uid == item.Uid);
//                 if (itemInList != null)
//                 {
//                     _items.Remove(itemInList);
//                     _items.Add(item);
//                 }
//             }
//             else if (type == WatchEventType.Deleted)
//             {
//                 var itemToDelete = _items.FirstOrDefault(x => x.Uid == item.Uid);
//                 if (itemToDelete != null)
//                 {
//                     _items.Remove(itemToDelete);
//                 }

//             }
//         }

//         abstract protected Task watcher(WatchEventType type, T item);

//         private void OnClosed()
//         {
//             var utc = DateTime.UtcNow.ToString();
//             var howlongran = (DateTime.UtcNow - lastrestart);
//             this._logger.LogError("===on watch {appName} Connection  Closed after  {howlongrunMin}:{howlongrunsec}  min:sec : re-running delay 30 seconds {utc}"
//             , appName, howlongran.TotalMinutes.ToString(), howlongran.Seconds.ToString(), utc);
//             Task.Delay(TimeSpan.FromSeconds(30)).Wait();
//             lastrestart = DateTime.UtcNow;
//             this._logger.LogError("=== on watch Restarting HealthCheckSubscriber Now....  time {time} ===", lastrestart.ToString());
//             executingTask = Task.Factory.StartNew(() => WatchStarter(cancellationToken), TaskCreationOptions.LongRunning);
//         }

//         private void OnError(Exception ex)
//         {
//             this._logger.LogError("===on watch Exception : DeploymentWatcherSyncService " + ex.Message);
//             this.ReportUnhealthy("Error " + ex.Message);
//         }




//         private void getAttributeDetails()
//         {
//             Type t = this.GetType();
//             // Get instance of the attribute.
//             K8sWatcherAttribute watcherAttribute =
//                 (K8sWatcherAttribute)Attribute.GetCustomAttribute(t, typeof(K8sWatcherAttribute));

//             if (watcherAttribute == null)
//             {
//                 _logger.LogWarning("RabbitMQSubscribeAttribute The attribute was not found. on {appName}", appName);
//             }
//             else
//             {
//                 var AppName = t.Name;
//                 string SubscribeName = string.IsNullOrEmpty(watcherAttribute.Name) ? AppName : AppName;

//                 if (watcherAttribute.TopicConfigurationSection != null)
//                 {
//                     if (_configuration[watcherAttribute.TopicConfigurationSection] != null)
//                     {
//                         //  topicName = _configuration[watcherAttribute.TopicConfigurationSection];
//                     }
//                     else
//                     {
//                         _logger.LogWarning("The attribute TopicConfigurationSection found. But the Topic was not found in Config section: {section}"
//                         , watcherAttribute.TopicConfigurationSection);
//                     }
//                 }
//                 // if (topicName == null)
//                 // {
//                 //     topicName = watcherAttribute.TopicName;
//                 // }
//                 if (watcherAttribute.TimeoutTotalMinutes > 0)
//                 {
//                     timeout = TimeSpan.FromMinutes(watcherAttribute.TimeoutTotalMinutes);
//                 }
//                 _logger.LogInformation("The Name Attribute is: {0}. Topic Name : {TopicName} Enabled : {Enabled}",
//                  watcherAttribute.Name, watcherAttribute.TopicName, watcherAttribute.Enabled);
//                 _logger.LogInformation("The Description Attribute is: {0}.", watcherAttribute.Description);
//             }
//         }
//         public void ReportHealthy(string message = "") => bgHealthCheck.ReportHealthy(message);
//         public void ReportUnhealthy(string message = "") => bgHealthCheck.ReportUnhealthy(message);
//         public void ReportDegraded(string message = "") => bgHealthCheck.ReportDegraded(message);

//         public override async Task StopAsync(CancellationToken cancellationToken)
//         {
//             _logger.LogInformation("Deployment Watch Hosted Service is stopping.");
//             await base.StopAsync(cancellationToken);
//         }

//         public override void Dispose()
//         {
//             _ResetEvent.Dispose();
//             _timer?.Dispose();
//             base.Dispose();
//         }

//     }

// }

