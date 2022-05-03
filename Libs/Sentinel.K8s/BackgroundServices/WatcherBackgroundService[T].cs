using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Sentinel.K8s.BackgroundServices
{
    public class WatcherBackgroundService<T> : BackgroundService
    {
        // private readonly IWatcher<T> _watcher;
        // private readonly IEnumerable<IWatcherHandler<T>> _handlers;

        // public WatcherBackgroundService(IWatcher<T> watcher, IEnumerable<IWatcherHandler<T>> handlers)
        // {
        //     _watcher = watcher;
        //     _handlers = handlers;
        // }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await Task.CompletedTask;
            //await _watcher.WatchAsync(stoppingToken, _handlers);
        }
    }

}