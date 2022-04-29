using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Sentinel.Models.Redis;
using Sentinel.Models.Scheduler;
using Sentinel.Redis;
using Sentinel.Scheduler.GeneralScheduler;
using StackExchange.Redis;

namespace Sentinel.Scheduler
{
    public class SchedulerRedisRepositoryFeeder<T> where T : IScheduledTask, new()
    {
        protected readonly ILogger<SchedulerRedisRepositoryFeeder<T>> _logger;
        protected readonly SchedulerRepository<T> _schedulerRepository;
        private readonly IConnectionMultiplexer _multiplexer;
        private RedisDictionary<T> redisDictionary = default!;
        private readonly string genericTypeName = typeof(T).Name;

        public SchedulerRedisRepositoryFeeder(
            SchedulerRepository<T> schedulerRepository,
            ILogger<SchedulerRedisRepositoryFeeder<T>> logger,
            IConnectionMultiplexer multiplexer)
        {
            _logger = logger;
            _schedulerRepository = schedulerRepository;
            _multiplexer = multiplexer;
        }

        public void Initiate(string redisKey)
        {
            redisDictionary = new RedisDictionary<T>(_multiplexer, _logger, redisKey);
        }

        public void Sync()
        {
            var ItemsInRedisButNotinRepo = redisDictionary.Keys.Where(redisKey => !_schedulerRepository.ScheduledTasks.Any(repo => repo.Task.Key == redisKey));
            var ItemsInRepoButNotinRedis = _schedulerRepository.ScheduledTasks.Where(repo => !redisDictionary.Keys.Any(redisKey => redisKey == repo.Task.Key));

            //Add items To Repo
            foreach (var itemKey in ItemsInRedisButNotinRepo)
            {
                var itm = redisDictionary[itemKey];
                _schedulerRepository.Add(itm);
            }

            //Remove removed items from Repo
            foreach (var item in ItemsInRepoButNotinRedis)
            {
                _schedulerRepository.ScheduledTasks.Remove(item);
                _logger.LogDebug($"SchedulerRepositoryFeeder : {genericTypeName} {item.Task.Key} Removed ");
            }

            foreach (var pair in redisDictionary)
            {
                if (pair.Value.Schedule != _schedulerRepository.ScheduledTasks.FirstOrDefault(x => x.Task.Key == pair.Key)?.Task.Schedule)
                {
                    _schedulerRepository.UpdateItem(pair.Value);
                    _logger.LogDebug($"SchedulerRepositoryFeeder : {genericTypeName} {pair.Key} updated  new Schedule {pair.Value.Schedule}");
                }
            }
            _logger.LogDebug("Repository Feeder" + genericTypeName + " : " + _schedulerRepository.ScheduledTasks.Count.ToString() + " items");
        }
    }
}