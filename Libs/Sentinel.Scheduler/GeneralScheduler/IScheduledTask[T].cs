namespace Sentinel.Scheduler.GeneralScheduler
{
    public interface IScheduledTask<T> where T : new()
    {
        string Schedule { get; }
        string Name { get; set; }
        string Namespace { get; set; }
        string Uid { get; set; }
        T Item { get; set; }

        //  IScaleDetails ScaleDetails { get; set; }
    }

    public interface IScaleDetails
    {
        //  int? ReplicaNumber { get; set; }
        //  ScaleUpDown ScaleUpDown { get; set; }
        //   string Timezone { get; set; }
    }
}