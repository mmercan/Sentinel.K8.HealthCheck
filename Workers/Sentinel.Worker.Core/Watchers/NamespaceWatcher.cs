using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Sentinel.K8s;
using Sentinel.K8s.BackgroundServices;
using Sentinel.K8s.Watchers;

namespace Sentinel.Worker.Core.Watchers
{
    [K8sWatcher(Name = "NamespaceWatcher", WatchAllNamespaces = true, Description = "Watches for Namespace changes", TimeoutTotalMinutes = 5, Enabled = true)]
    public class NamespaceWatcher : WatcherBackgroundService<V1Namespace>
    {


        // protected override Task watcher(WatchEventType type, k8s.Models.V1Namespace item)
        // {
        //     _logger.LogInformation("Namespace watcher event: {type} :  {name}", type, item.Metadata.Name);
        //     return Task.CompletedTask;
        // }
        public NamespaceWatcher(IConfiguration configuration, IKubernetesClient client,
        ILogger<WatcherBackgroundService<V1Namespace>> logger, IOptions<HealthCheckServiceOptions> hcoptions)
        : base(configuration, client, logger, hcoptions)
        {
        }

        public override void Watch(WatchEventType Event, V1Namespace Resource)
        {
            _logger.LogInformation("Namespace watcher event: {type} :  {name}", Event, Resource.Metadata.Name);
        }
    }
}