using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Sentinel.Redis
{
    public class RedisNamespaceStore<T>
    {

        private readonly IConnectionMultiplexer _multiplexer;
        private readonly IDatabase database;
        private readonly IServer server;
        private readonly string prefix;

        public RedisNamespaceStore(IConnectionMultiplexer multiplexer, string prefix)
        {
            _multiplexer = multiplexer;

            database = _multiplexer.GetDatabase();
            server = _multiplexer.GetServer(multiplexer.GetEndPoints().First());
            this.prefix = prefix;

            // JsonConvert.DeserializeObject()
        }

        public string Status()
        {
            return _multiplexer.GetStatus();
        }

        public async Task<T> GetAsync(string key)
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

        public IAsyncEnumerable<T> GetAsyncEnumerable() => GetAsyncEnumerable(this.prefix);
        public async IAsyncEnumerable<T> GetAsyncEnumerable(string pattern)
        {
            var keys = server.Keys(pattern: pattern).ToArray();
            var values = await database.StringGetAsync(keys);

            if (!values.Any())
            {
                yield return default;
            }
            else
            {
                foreach (var item in values)
                {
                    yield return JsonConvert.DeserializeObject<T>(item);
                }
            }
        }


        public async Task<IList<T>> GetListAsync() => await GetListAsync(this.prefix);
        public async Task<IList<T>> GetListAsync(string pattern)
        {
            var items = new List<T>();
            var keys = server.Keys(pattern: pattern).ToArray();
            var values = await database.StringGetAsync(keys);

            if (!values.Any())
            {
                return default;
            }
            else
            {
                foreach (var itemStr in values)
                {
                    var item = JsonConvert.DeserializeObject<T>(itemStr);
                    items.Add(item);
                }
                return items;
            }
        }

        public async Task<bool> SetAsync(string key, T item)
        {
            var stringValue = JsonConvert.SerializeObject(item);
            return await database.StringSetAsync(key, stringValue);
        }

        //GetAll
        //Upsert

        //Delete
        //Update
        //Insert


        //Namespace:{{namespaceName}}

        //Service:{{namespace}}:{{servicename}}

        //Deployment:{{namespace}}:{{deploymentname}}
    }
}