using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sentinel.Scheduler
{
    public interface ISchedulerRepositoryFeeder
    {
        void Sync();
    }
}