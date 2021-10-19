namespace Sentinel.Scheduler.GeneralScheduler
{
    public interface IScheduledTask<T>
    {
        string Uid { get; }

        string Key { get; set; }
        string Schedule { get; }
    }
}