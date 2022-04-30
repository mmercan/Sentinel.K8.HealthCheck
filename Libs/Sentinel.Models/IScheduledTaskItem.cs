namespace Sentinel.Models.Scheduler
{
    public interface IScheduledTaskItem
    {
        string Uid { get; }

        string Key { get; }
        string Schedule { get; }
    }
}