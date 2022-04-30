using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Libs.Sentinel.Scheduler
{
    public class RedisKeyFeederOption<T>
    {
        public string RedisKey { get; set; } = default!;
    }
}