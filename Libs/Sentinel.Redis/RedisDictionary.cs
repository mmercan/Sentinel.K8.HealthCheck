using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using Sentinel.Models.Redis;
using StackExchange.Redis;

namespace Sentinel.Redis
{


#pragma warning disable CS8604
#pragma warning disable CS8603
#pragma warning disable CS8601
#pragma warning disable CS8620
    //Key value has to be string Type
    public class RedisDictionary<TValue> : IRedisDictionary<TValue>
    {
        private readonly IDatabase database;
        private readonly ILogger _logger;
        private readonly string _redisKey;
        private readonly Polly.Policy policy;
        public RedisDictionary(IConnectionMultiplexer multiplexer, ILogger logger, string redisKey)
        {
            _redisKey = redisKey;

            Logger = logger;
            database = multiplexer.GetDatabase();
            _logger = logger;


            policy = Policy.Handle<RedisTimeoutException>().Or<RedisConnectionException>()
                    .WaitAndRetry(new[] {
                        TimeSpan.FromSeconds(1),TimeSpan.FromSeconds(2),TimeSpan.FromSeconds(3)
                    }, (ex, timeSpan, retryCount, context) =>
                    {
                        _logger.LogError(ex, "RedisDictionary : Polly retry " + retryCount.ToString() + "  Error");
                        if (retryCount == 2) { throw ex; }
                        // Add logic to be executed before each retry, such as logging    
                    });
            // policy.Execute(() =>
            // {
            //     var service = taskThatShouldRun.Item.FindServiceRelatedtoHealthCheckResourceV1(redisServiceDictionary);
            //     taskThatShouldRun.Item.RelatedService = service;
            //     if (service == null)
            //     {
            //         _logger.LogCritical("BusScheduler : Error Finding Service Related to HealthCheckResourceV1 Logged in RedisHealCheckServiceNotFoundDictionary");
            //         redisHealCheckServiceNotFoundDictionary.Add(taskThatShouldRun.Item);
            //     }
            // });

        }

        private static string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
        private static T? Deserialize<T>(string serialized)
        {
            return JsonConvert.DeserializeObject<T>(serialized);
        }

        public void Add(TValue value) => Add(PropertyInfoHelpers.GetKeyValue<string, TValue>(value), value);
        public void Add(string key, TValue value)
        {
            policy.Execute(() =>
            {
                database.HashSet(_redisKey, key, Serialize(value));
            });

        }



        public async Task AddAsync(TValue value) => await AddAsync(PropertyInfoHelpers.GetKeyValue<string, TValue>(value), value);
        public async Task AddAsync(string key, TValue value)
        {
            await policy.Execute(async () =>
            {
                await database.HashSetAsync(_redisKey, key, Serialize(value));
            });
        }

        public bool ContainsKey(TValue value) => ContainsKey(PropertyInfoHelpers.GetKeyValue<string, TValue>(value));
        public bool ContainsKey(string key)
        {
            var result = false;
            policy.Execute(() =>
           {
               result = database.HashExists(_redisKey, key);
           });
            return result;
        }

        public bool Remove(KeyValuePair<string, TValue> item) => Remove(item.Key);
        public bool Remove(TValue value) => Remove(PropertyInfoHelpers.GetKeyValue<string, TValue>(value));
        public bool Remove(string key)
        {
            var result = false;
            policy.Execute(() =>
           {
               result = database.HashDelete(_redisKey, key);
           });
            return result;
        }

        public bool TryGetValue(string key, out TValue value)
        {
            RedisValue redisValue = new RedisValue();
            policy.Execute(() =>
            {
                redisValue = database.HashGet(_redisKey, key);
            });
            if (redisValue.IsNull)
            {
                _logger.LogInformation(key + " Value not found");
                value = default(TValue);
                return false;
            }
            value = Deserialize<TValue>(redisValue);
            return true;
        }
        public ICollection<TValue> Values
        {
            get { return getValues(); }
        }

        private ICollection<TValue> getValues()
        {
            ICollection<TValue>? values = null;
            policy.Execute(() =>
            {
                values = new Collection<TValue>(database.HashValues(_redisKey).Select(h => Deserialize<TValue>(h.ToString())).ToList());
            });
            return values;
        }
        public ICollection<string> Keys
        {
            get { return getKeys(); }
        }
        private ICollection<string> getKeys()
        {
            ICollection<string>? keys = null;
            policy.Execute(() =>
            {
                keys = new Collection<string>(database.HashKeys(_redisKey).Select(h => h.ToString()).ToList());
            });
            return keys;
        }

        public TValue this[string key]
        {
            get
            {
                var redisValue = database.HashGet(_redisKey, key);
                return redisValue.IsNull ? default(TValue) : Deserialize<TValue>(redisValue.ToString());
            }
            set
            {
                Add(key, value);
            }
        }

        private string originalValue(string key)
        {
            var redisValue = database.HashGet(_redisKey, key);
            return redisValue.IsNull ? null : redisValue.ToString();
        }

        public void Add(KeyValuePair<string, TValue> item) => Add(item.Key, item.Value);

        public void Clear()
        {
            database.KeyDelete(_redisKey);
        }
        public bool Contains(KeyValuePair<string, TValue> item)
        {
            return database.HashExists(_redisKey, item.Key);
        }
        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
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


        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            var db = database;
            foreach (var hashKey in db.HashKeys(_redisKey))
            {
                var redisValue = db.HashGet(_redisKey, hashKey);
                yield return new KeyValuePair<string, TValue>(hashKey, Deserialize<TValue>(redisValue.ToString()));
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
        public void AddMultiple(IEnumerable<KeyValuePair<string, TValue>> items)
        {
            database.HashSet(_redisKey, items.Select(i => new HashEntry(i.Key, Serialize(i.Value))).ToArray());
        }


// overrideExisting param added tp Sync
        // public void UpSert(IEnumerable<TValue> items)
        // {
        //     foreach (var item in items)
        //     {
        //         Add(item);
        //     }

        //     foreach (var key in Keys)
        //     {
        //         if (!items.Any(p => PropertyInfoHelpers.GetKeyValue<string, TValue>(p) == key))
        //         {
        //             Remove(key);
        //         }
        //     }
        // }

        public void Sync(IEnumerable<TValue> items, bool overrideExisting = true)
        {
            var keys = Keys;
            foreach (var item in items)
            {
                if (overrideExisting)
                {
                    Add(item);
                }
                else
                {
                    string? itemKey = PropertyInfoHelpers.GetKeyValue<string, TValue>(item);
                    if (!keys.Any(p => p == itemKey))
                    {
                        Add(item);
                    }
                }
            }

            foreach (var key in Keys)
            {
                if (!items.Any(p => PropertyInfoHelpers.GetKeyValue<string, TValue>(p) == key))
                {
                    Remove(key);
                }
            }

        }

        public void Sync(IEnumerable<TValue> items, Func<TValue, string> keyFunc)
        {
            var keys = Keys;
            foreach (var item in items)
            {
                var key = keyFunc.Invoke(item);
                if (!keys.Any(p => p == key))
                {
                    Add(key, item);
                }
            }

            foreach (var key in Keys)
            {
                if (!items.Any(p => keyFunc.Invoke(p) == key))
                {
                    Remove(key);
                }
            }
        }

        // private void UpdatePropertiesExceptDateTime(IEnumerable<TValue> items)
        // {
        //     foreach (var item in items)
        //     {


        //             var newValue = item;
        //             var properties = PropertyInfoHelpers.GetProperties<TValue>();
        //             foreach (var property in properties)
        //             {
        //                 if (property.GetType() != typeof(DateTime))
        //                 {
        //                     var originalValue = property.GetValue(original);
        //                     var newValueValue = property.GetValue(newValue);
        //                     if (originalValue != newValueValue)
        //                     {
        //                         property.SetValue(original, newValueValue);
        //                     }
        //                 }
        //             }
        //         }
        //     }
        // }

    }
}