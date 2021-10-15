namespace Sentinel.Scheduler.GeneralScheduler
{
    public class ScheduledTask<T> : IScheduledTask<T> where T : new()
    {
        public string Name { get; set; }

        public string Namespace { get; set; }
        public string Schedule { get; set; }
        public string Uid { get; set; }
        public T Item { get; set; }
    }
}