using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Sentinel.Worker.Sync.RedisHelpers
{
    public class RedisDictionary<TKey, TValue> : IDictionary<TKey, TValue>
    {
        private readonly IConnectionMultiplexer _multiplexer;
        private readonly IDatabase database;
        private readonly ILogger _logger;
        private string _redisKey;
        public RedisDictionary(IConnectionMultiplexer multiplexer, ILogger logger, string redisKey)
        {
            _redisKey = redisKey;
            _multiplexer = multiplexer;
            Logger = logger;
            database = _multiplexer.GetDatabase();
            _logger = logger;
        }

        private string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        private T Deserialize<T>(string serialized)
        {
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        public void Add(TValue value) => Add(PropertyInfoHelpers.GetKeyValue<TKey, TValue>(value), value);
        public void Add(TKey key, TValue value)
        {
            database.HashSet(_redisKey, Serialize(key), Serialize(value));
        }


        public async Task AddAsync(TValue value) => await AddAsync(PropertyInfoHelpers.GetKeyValue<TKey, TValue>(value), value);
        public async Task AddAsync(TKey key, TValue value)
        {
            await database.HashSetAsync(_redisKey, Serialize(key), Serialize(value));
        }

        public void ContainsKey(TValue value) => ContainsKey(PropertyInfoHelpers.GetKeyValue<TKey, TValue>(value));
        public bool ContainsKey(TKey key)
        {
            return database.HashExists(_redisKey, Serialize(key));
        }

        public void Remove(TValue value) => Remove(PropertyInfoHelpers.GetKeyValue<TKey, TValue>(value));
        public bool Remove(TKey key)
        {
            return database.HashDelete(_redisKey, Serialize(key));
        }
        public bool TryGetValue(TKey key, out TValue value)
        {
            var redisValue = database.HashGet(_redisKey, Serialize(key));
            if (redisValue.IsNull)
            {
                value = default(TValue);
                return false;
            }
            value = Deserialize<TValue>(redisValue.ToString());
            return true;
        }
        public ICollection<TValue> Values
        {
            get { return new Collection<TValue>(database.HashValues(_redisKey).Select(h => Deserialize<TValue>(h.ToString())).ToList()); }
        }
        public ICollection<TKey> Keys
        {
            get { return new Collection<TKey>(database.HashKeys(_redisKey).Select(h => Deserialize<TKey>(h.ToString())).ToList()); }
        }
        public TValue this[TKey key]
        {
            get
            {
                var redisValue = database.HashGet(_redisKey, Serialize(key));
                return redisValue.IsNull ? default(TValue) : Deserialize<TValue>(redisValue.ToString());
            }
            set
            {
                Add(key, value);
            }
        }
        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);

        public void Clear()
        {
            database.KeyDelete(_redisKey);
        }
        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            return database.HashExists(_redisKey, Serialize(item.Key));
        }
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            database.HashGetAll(_redisKey).CopyTo(array, arrayIndex);
        }
        public int Count
        {
            get { return (int)database.HashLength(_redisKey); }
        }
        public bool IsReadOnly
        {
            get { return false; }
        }

        public ILogger Logger { get; }

        public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key);


        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            var db = database;
            foreach (var hashKey in db.HashKeys(_redisKey))
            {
                var redisValue = db.HashGet(_redisKey, hashKey);
                yield return new KeyValuePair<TKey, TValue>(Deserialize<TKey>(hashKey.ToString()), Deserialize<TValue>(redisValue.ToString()));
            }
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            yield return GetEnumerator();
        }

        public void AddMultiple(IEnumerable<TValue> items)
        {
            foreach (var item in items)
            {
                Add(item);
            }
        }
        public void AddMultiple(IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            database.HashSet(_redisKey, items.Select(i => new HashEntry(Serialize(i.Key), Serialize(i.Value))).ToArray());
        }


        public void Sync(IEnumerable<TValue> items)
        {
            var keys = Keys;
            foreach (var item in items)
            {
                string itemKey = PropertyInfoHelpers.GetKeyValue<TKey, TValue>(item).ToJSON();
                if (!keys.Any(p => p.ToJSON() == itemKey))
                {
                    Add(item);
                }
            }

            foreach (var key in Keys)
            {
                if (!items.Any(p => PropertyInfoHelpers.GetKeyValue<TKey, TValue>(p).ToJSON() == key.ToJSON()))
                {
                    Remove(key);
                }
            }

        }


    }
}