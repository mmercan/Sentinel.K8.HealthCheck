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
    public class SchedulerRepository<T> where T : IScheduledTask, new()
    {
        public ObservableCollection<T> Items { get; set; }
        public List<SchedulerTaskWrapper<T>> ScheduledTasks { get; }
        private readonly ILogger<SchedulerRepository<T>> _logger;
        private readonly string genericTypeName;

        public SchedulerRepository(ILogger<SchedulerRepository<T>> logger)
        {
            genericTypeName = typeof(T).Name;
            _logger = logger;
            Items = new ObservableCollection<T>();
            ScheduledTasks = new List<SchedulerTaskWrapper<T>>();


            Items.ForEach(p => addItem(p));

            Items.CollectionChanged += new NotifyCollectionChangedEventHandler(collectionChanged);
        }

        private void collectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (T x in e.NewItems) { addItem(x); }
            }
            if (e.OldItems != null)
            {
                foreach (T y in e.OldItems) { deleteItem(y); }
            }
            // if (e.Action == NotifyCollectionChangedAction.Move) { }
        }

        private void addItem(T item)
        {
            var referenceTime = DateTime.UtcNow;

            var scheduledTask = new SchedulerTaskWrapper<T>
            {
                Uid = item.Uid,
                Schedule = CrontabSchedule.Parse(item.Schedule),
                Task = item,
                Item = item,
                NextRunTime = referenceTime,
            };

            ScheduledTasks.Add(scheduledTask);
            _logger.LogCritical("SchedulerRepository Added" + genericTypeName + " Key : " + scheduledTask.Task.Key + " : " + scheduledTask.Schedule.ToString() + " ===> " + scheduledTask.Schedule.GetNextOccurrence(referenceTime).ToString("MM/dd/yyyy H:mm"));
        }

        public void UpdateItem(T item)
        {
            // var itemToUpdateScheduled = ScheduledTasks.FirstOrDefault(e => e.Uid == item.Uid);
            // var itemToUpdate = Items.FirstOrDefault(e => e.Uid == item.Uid);

            var referenceTime = DateTime.UtcNow;
            var scheduledTask = new SchedulerTaskWrapper<T>
            {
                Uid = item.Uid,
                Schedule = CrontabSchedule.Parse(item.Schedule),
                Task = item,
                Item = item,
                NextRunTime = referenceTime,
            };

            var itemIndex = Items.IndexOf(Items.FirstOrDefault(e => e.Uid == item.Uid));
            if (itemIndex > -1)
            {
                Items[itemIndex] = item;
            }


            var scheduledIndex = ScheduledTasks.IndexOf(ScheduledTasks.FirstOrDefault(e => e.Uid == item.Uid));
            if (scheduledIndex > -1)
            {
                ScheduledTasks[scheduledIndex] = scheduledTask;
            }
            _logger.LogCritical("SchedulerRepository Updated" + genericTypeName + " Key : " + scheduledTask.Task.Key + " : " + scheduledTask.Schedule.ToString() + " ===> " + scheduledTask.Schedule.GetNextOccurrence(referenceTime).ToString("MM/dd/yyyy H:mm"));
        }

        private void deleteItem(T item)
        {
            var itemtodelete = ScheduledTasks.FirstOrDefault(e => e.Uid == item.Uid);
            if (itemtodelete != null)
            {
                ScheduledTasks.Remove(itemtodelete);
            }
            _logger.LogCritical("SchedulerRepository Deleted" + genericTypeName + " Key : " + item.Key);
        }

    }
}