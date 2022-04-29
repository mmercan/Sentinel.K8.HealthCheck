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
        // public ObservableCollection<T> Items { get; set; }
        public List<ScheduledTask<T>> ScheduledTasks { get; }
        private readonly ILogger<SchedulerRepository<T>> _logger;
        private readonly string genericTypeName;
        public SchedulerRepository(ILogger<SchedulerRepository<T>> logger)
        {
            genericTypeName = typeof(T).Name;
            _logger = logger;
            // Items = new ObservableCollection<T>();
            ScheduledTasks = new List<ScheduledTask<T>>();
            // Items.ForEach(p => addItem(p));
            // Items.CollectionChanged += new NotifyCollectionChangedEventHandler(collectionChanged);
        }

        // private void collectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        // {
        //     if (e.NewItems != null)
        //     {
        //         foreach (T x in e.NewItems) { addItem(x); }
        //     }
        //     if (e.OldItems != null)
        //     {
        //         foreach (T y in e.OldItems) { deleteItem(y); }
        //     }
        //     // if (e.Action == NotifyCollectionChangedAction.Move) { }
        // }

        public void UpdateItem(T item)
        {
            if (item == null) { throw new ArgumentNullException("item"); }

            var referenceTime = DateTime.UtcNow;
            var scheduledTask = new ScheduledTask<T>(_logger, item, referenceTime = DateTime.UtcNow);

            // var selecteditem = ScheduledTasks.FirstOrDefault(e => e.Uid == item.Uid);
            // if (selecteditem != null)
            // {
            //     var itemIndex = ScheduledTasks.IndexOf(selecteditem);
            //     if (itemIndex > -1)
            //     {
            //         ScheduledTasks[itemIndex] = new ScheduledTask<T>(_logger, item);
            //     }
            // }
            // else { _logger.LogCritical("SchedulerRepository <{genericTypeName}>  Item  not Found in ScheduledTasks UID : {Uid} ", genericTypeName, item.Uid); }

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
    }
}