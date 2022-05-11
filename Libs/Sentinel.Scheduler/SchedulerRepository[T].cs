using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.Extensions.Logging;
using Sentinel.Models.Scheduler;
using Sentinel.Scheduler.GeneralScheduler;
using Sentinel.Scheduler.GeneralScheduler.Cron;

namespace Sentinel.Scheduler
{
    public class SchedulerRepository<T> : ISchedulerRepository where T : IScheduledTaskItem
    {

        // public IScheduledTaskItem getitem(T value)
        // {
        //     return value;
        // }

        // public IList<IScheduledTaskItem> getitems(IList<T> values)
        // {
        //     return values;
        // }
        // public IScheduledTaskItem[] getitems(T[] values)
        // {
        //     return values;
        // }

        public List<ScheduledTask<T>> ScheduledTasks { get; }
        public List<IScheduledTask> IScheduledTasks
        {
            get
            {
                var res = ScheduledTasks.Cast<IScheduledTask>().ToList();
                return res;
            }
        }
        private readonly ILogger<SchedulerRepository<T>> _logger;
        private readonly string genericTypeName;
        public SchedulerRepository(ILogger<SchedulerRepository<T>> logger)
        {
            genericTypeName = typeof(T).Name;
            _logger = logger;
            ScheduledTasks = new List<ScheduledTask<T>>();
        }
        public void UpdateSchedulerRepository(T item)
        {
            if (item == null) { throw new ArgumentNullException("item"); }

            var referenceTime = DateTime.UtcNow;
            var scheduledTask = new ScheduledTask<T>(_logger, item, referenceTime = DateTime.UtcNow);

            var scheduledSelectedTask = ScheduledTasks.FirstOrDefault(e => e.Uid == item.Uid);
            if (scheduledSelectedTask != null)
            {
                var scheduledIndex = ScheduledTasks.IndexOf(scheduledSelectedTask);
                if (scheduledIndex > -1)
                {
                    ScheduledTasks[scheduledIndex] = scheduledTask;
                }
                _logger.LogInformation("SchedulerRepository Updated" + genericTypeName + " Key : " + scheduledTask.Task.Key + " : " + scheduledTask.Schedule.ToString() + " ===> " + scheduledTask.Schedule.GetNextOccurrence(referenceTime).ToString("MM/dd/yyyy H:mm"));
            }
            else { _logger.LogCritical("SchedulerRepository <" + genericTypeName + " >  Item  not Found in ScheduledTasks UID : " + item.Uid); }
        }

        public void UpdateTask(T item)
        {
            var referenceTime = DateTime.UtcNow;
            var scheduledSelectedTask = ScheduledTasks.FirstOrDefault(e => e.Uid == item.Uid);
            if (scheduledSelectedTask != null)
            {
                var scheduledIndex = ScheduledTasks.IndexOf(scheduledSelectedTask);
                if (scheduledIndex > -1)
                {
                    // if (ScheduledTasks[scheduledIndex].Task.ToJSON() != item.ToJSON())
                    // {
                    ScheduledTasks[scheduledIndex].Task = item;
                    _logger.LogDebug("SchedulerRepository Updated " + genericTypeName + " Key : " + item.Key + " : " + item.Schedule.ToString() + " ===> " + scheduledSelectedTask.Schedule.GetNextOccurrence(referenceTime).ToString("MM/dd/yyyy H:mm"));
                    // }
                }

            }
        }
        public void Add(T item)
        {
            var referenceTime = DateTime.UtcNow;
            var scheduledTask = new ScheduledTask<T>(_logger, item, referenceTime);

            ScheduledTasks.Add(scheduledTask);
            _logger.LogInformation("SchedulerRepository Added " + genericTypeName + " Key : " + scheduledTask.Task.Key + " : " + scheduledTask.Schedule.ToString() + " ===> " + scheduledTask.Schedule.GetNextOccurrence(referenceTime).ToString("MM/dd/yyyy H:mm"));
        }
        public void Remove(T item)
        {
            var itemtodelete = ScheduledTasks.FirstOrDefault(e => e.Uid == item.Uid);
            if (itemtodelete != null)
            {
                ScheduledTasks.Remove(itemtodelete);
            }
            _logger.LogInformation("SchedulerRepository Deleted" + genericTypeName + " Key : " + item.Key);
        }


        public void Sync(IList<T>? items)
        {
            if (items == null)
            {
                _logger.LogDebug("SchedulerRepository<{type}> Sync null", genericTypeName);
                return;
            }

            var itemsToAdd = items.Where(p => !ScheduledTasks.Any(e => e.Uid == p.Uid)).ToList();//.ForEach(p => Add(p));
            var itemsToUpdate = items.Where(p => ScheduledTasks.Any(e => e.Uid == p.Uid)).ToList();//.ForEach(p => Add(p));
            var itemsToRemove = ScheduledTasks.Where(p => !items.Any(e => e.Uid == p.Uid)).ToList();//.ForEach(p => Add(p));

            foreach (var item in itemsToAdd)
            {
                Add(item);
                _logger.LogDebug($"SchedulerRepositoryFeeder : {genericTypeName} {item.Key} Removed ");
            }


            foreach (var item in itemsToRemove)
            {
                ScheduledTasks.Remove(item);
                _logger.LogDebug($"SchedulerRepositoryFeeder : {genericTypeName} {item.Task.Key} Removed ");
            }

            foreach (var pair in itemsToUpdate)
            {

                if (pair.Schedule != ScheduledTasks.FirstOrDefault(x => x.Task.Key == pair.Key)?.Task.Schedule)
                {
                    UpdateSchedulerRepository(pair);
                    _logger.LogDebug($"SchedulerRepositoryFeeder : {genericTypeName} {pair.Key} updated  new Schedule {pair.Schedule}");
                }
                else
                {
                    UpdateTask(pair);
                }
            }

        }
    }
}