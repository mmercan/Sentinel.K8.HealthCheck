using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.PubSub.BackgroundServices
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class RabbitMQSubscribeAttribute : Attribute
    {

        public string Name { get; set; } = default!;

        public string TopicName { get; set; } = default!;

        public string TopicConfigurationSection { get; set; } = default!;

        public string Description { get; set; } = default!;

        public bool Enabled { get; set; } = true;
    }
}