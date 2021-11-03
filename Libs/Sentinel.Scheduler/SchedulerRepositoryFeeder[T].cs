using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Sentinel.Models.Redis;
using Sentinel.Models.Scheduler;
using Sentinel.Redis;
using StackExchange.Redis;

namespace Sentinel.Scheduler
{
    public class SchedulerRepositoryFeeder<T> where T : IScheduledTask, new()
    {
        protected readonly ILogger<SchedulerRepositoryFeeder<T>> _logger;
        protected readonly SchedulerRepository<T> _schedulerRepository;
        protected readonly IRedisDictionary<T> _redisDictionary;
        private readonly IConnectionMultiplexer _multiplexer;
        private RedisDictionary<T> redisDictionary;
        private readonly string genericTypeName = typeof(T).Name;

        public SchedulerRepositoryFeeder(
            SchedulerRepository<T> schedulerRepository,
            ILogger<SchedulerRepositoryFeeder<T>> logger,
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
            var ItemsInRedisButNotinRepo = redisDictionary.Keys.Where(redisKey => !_schedulerRepository.Items.Any(repo => repo.Key == redisKey));
            var ItemsInRepoButNotinRedis = _schedulerRepository.Items.Where(repo => !redisDictionary.Keys.Any(redisKey => redisKey == repo.Key));

            //Add items To Repo
            foreach (var itemKey in ItemsInRedisButNotinRepo)
            {
                var itm = redisDictionary[itemKey];
                _schedulerRepository.Items.Add(itm);
            }

            //Remove removed items from Repo
            foreach (var item in ItemsInRepoButNotinRedis)
            {
                _schedulerRepository.Items.Remove(item);
            }

            foreach (var pair in redisDictionary)
            {
                if (pair.Value.Schedule != _schedulerRepository.Items.FirstOrDefault(x => x.Key == pair.Key)?.Schedule)
                {
                    _schedulerRepository.UpdateItem(pair.Value);
                }
            }
            _logger.LogDebug("Repository Feeder" + genericTypeName + " : " + _schedulerRepository.Items.Count.ToString() + " items");
        }


    }
}