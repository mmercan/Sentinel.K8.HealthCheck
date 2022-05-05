using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.K8s.BackgroundServices
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class K8sWatcherAttribute : Attribute
    {

        public string Name { get; set; } = default!;

        public Boolean WatchAllNamespaces { get; set; } = true!;
        public string Namespace { get; set; } = default!;

        public string Description { get; set; } = default!;

        public int TimeoutTotalMinutes { get; set; } = default!;
        public bool Enabled { get; set; } = true;
        public ushort WatcherHttpTimeout { get; set; } = 60;


    }
}