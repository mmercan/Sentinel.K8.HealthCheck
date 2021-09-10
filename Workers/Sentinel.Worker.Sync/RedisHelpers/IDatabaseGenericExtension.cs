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
    }
}