using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Sentinel.Worker.Sync.RedisHelpers
{
    public static class IDatabaseGenericExtension
    {
        public static T Get<T>(this IDatabase database, string key)
        {
            var value = database.StringGet(key);
            if (!value.HasValue)
            {
                return default;
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
        }

        public static async Task<T> GetAsync<T>(this IDatabase database, string key)
        {
            var value = await database.StringGetAsync(key);
            if (!value.HasValue)
            {
                return default;
            }
            else
            {
                return JsonConvert.DeserializeObject<T>(value);
            }
        }


        public static bool Set<T>(this IDatabase database, string key, T value)
        {
            var stringValue = JsonConvert.SerializeObject(value);
            return database.StringSet(key, stringValue);
        }

        public static Task<bool> SetAsync<T>(this IDatabase database, string key, T value)
        {
            var stringValue = JsonConvert.SerializeObject(value);
            return database.StringSetAsync(key, stringValue);
        }

        public async static Task<IList<T>> SetListAsync<T>(this IDatabase database, IList<T> items, Func<T, string> keyFunc)
        {
            foreach (var item in items)
            {
                var key = keyFunc.Invoke(item);
                await database.SetAsync(key, item);
            }
            return items;
        }


        public async static Task<List<T>> SetListAsync<T>(this IDatabase database, List<T> items)
        {
            var keyProp = typeof(T).GetProperties().SingleOrDefault(p => p.GetCustomAttributes(typeof(KeyAttribute), false).Count() > 0);

            if (keyProp == null)
            {
                throw new ArgumentException("KeyAttribute is mising for " + typeof(T).GetType().ToString());
            }

            foreach (var item in items)
            {
                var key = keyProp.GetValue(item).ToString();
                await database.SetAsync(key, item);
            }
            return items;
        }

    }
}