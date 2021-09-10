using Newtonsoft.Json;
using StackExchange.Redis;

namespace Sentinel.Worker.Sync
{
    public class RedisNamespaceStore
    {

        private readonly IConnectionMultiplexer _multiplexer;
        private readonly IDatabase _db;
        public RedisNamespaceStore(IConnectionMultiplexer multiplexer)
        {


            _multiplexer = multiplexer;
            if (!_multiplexer.IsConnected)
            {
                // _multiplexer.conn

            }
            _db = _multiplexer.GetDatabase();
            // JsonConvert.DeserializeObject()
        }

        public string Status()
        {
            return _multiplexer.GetStatus();
        }

        //Get

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