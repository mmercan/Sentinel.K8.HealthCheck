namespace Sentinel.Scheduler
{
    public class SchedulerRepositoryFeederOptions<T>
    {
        public string Cron { get; set; }
        public string RedisKey { get; set; }
    }
}