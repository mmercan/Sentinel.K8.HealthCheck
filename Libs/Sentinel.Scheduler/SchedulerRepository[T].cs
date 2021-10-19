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
        public ObservableCollection<T> Items { get => items; set => items = value; }
        public List<SchedulerTaskWrapper<T>> ScheduledTasks { get; }

        private ILogger<SchedulerRepository<T>> logger;
        private ObservableCollection<T> items;

        public SchedulerRepository(ILogger<SchedulerRepository<T>> logger)
        {
            this.logger = logger;
            Items = new ObservableCollection<T>();
            ScheduledTasks = new List<SchedulerTaskWrapper<T>>();

            this.logger = logger;
            foreach (T item in Items)
            {
                addItem(item);
            }
            Items.CollectionChanged += new NotifyCollectionChangedEventHandler(collectionChanged);
        }

        private void collectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (T x in e.NewItems) { addItem(x); };
            };
            if (e.OldItems != null)
            {
                foreach (T y in e.OldItems) { deleteItem(y); }
            }
            if (e.Action == NotifyCollectionChangedAction.Move) { }
        }

        private void addItem(T item)
        {
            var referenceTime = DateTime.UtcNow;

            var scheduledTask = new SchedulerTaskWrapper<T>
            {
                Uid = item.Uid,
                Schedule = CrontabSchedule.Parse(item.Schedule),
                Task = item,
                NextRunTime = referenceTime,

            };

            ScheduledTasks.Add(scheduledTask);
            logger.LogCritical(scheduledTask.Task.Key + " : " + scheduledTask.Schedule.ToString() + " ===> " + scheduledTask.Schedule.GetNextOccurrence(referenceTime).ToString("MM/dd/yyyy H:mm"));
        }

        private void editItem(T item)
        {
            var itemToUpdate = ScheduledTasks.FirstOrDefault(e => e.Uid == item.Uid);
            var referenceTime = DateTime.UtcNow;

            var scheduledTask = new SchedulerTaskWrapper<T>
            {
                Uid = item.Uid,
                Schedule = CrontabSchedule.Parse(item.Schedule),
                Task = item,
                NextRunTime = referenceTime,

            };
            itemToUpdate = scheduledTask;
        }

        private void deleteItem(T item)
        {
            var itemtodelete = ScheduledTasks.FirstOrDefault(e => e.Uid == item.Uid);
            if (itemtodelete != null)
            {
                ScheduledTasks.Remove(itemtodelete);
            }
        }

    }
}