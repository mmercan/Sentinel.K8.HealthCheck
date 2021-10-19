namespace Sentinel.Models.Scheduler
{
    public interface IScheduledTask
    {
        string Uid { get; }

        string Key { get; }
        string Schedule { get; }
    }
}