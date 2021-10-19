using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Quartz;
using Sentinel.Models.Redis;
using Sentinel.Models.Scheduler;


namespace Sentinel.Scheduler
{
    public class SchedulerRepositoryFeeder<T, TKey>
    where T : IScheduledTask, new()

    {
        protected readonly ILogger<SchedulerRepositoryFeeder<T, TKey>> _logger;
        protected readonly SchedulerRepository<T> _schedulerRepository;
        protected readonly IRedisDictionary<TKey, T> _redisDictionary;

        public SchedulerRepositoryFeeder(
            SchedulerRepository<T> schedulerRepository,
            ILogger<SchedulerRepositoryFeeder<T, TKey>> logger,
            IRedisDictionary<TKey, T> redisDictionary
            )
        {
            _logger = logger;
            _schedulerRepository = schedulerRepository;
            _redisDictionary = redisDictionary;
        }

        public void Sync()
        {
            var ItemsInRedisButNotinRepo = _redisDictionary.Keys.Where(redisKey => !_schedulerRepository.Items.Any(repo => repo.Key == redisKey.ToJSON()));
            var ItemsInRepoButNotinRedis = _schedulerRepository.Items.Where(repo => !_redisDictionary.Keys.Any(redisKey => redisKey.ToJSON() == repo.Key));

            //Add items To Repo
            foreach (var itemKey in ItemsInRedisButNotinRepo)
            {
                var itm = _redisDictionary[itemKey];
                _schedulerRepository.Items.Add(itm);
            }

            //Remove removed items from Repo
            foreach (var item in ItemsInRepoButNotinRedis)
            {
                _schedulerRepository.Items.Remove(item);
            }
        }


    }
}