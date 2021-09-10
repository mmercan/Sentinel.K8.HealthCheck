using Newtonsoft.Json;
using StackExchange.Redis;

namespace Sentinel.Worker.Sync
{
    public class RedisNamespaceStore
    {

        private readonly IConnectionMultiplexer _multiplexer;
        public IDatabase Database { get; }
        public RedisNamespaceStore(IConnectionMultiplexer multiplexer)
        {


            _multiplexer = multiplexer;
            if (!_multiplexer.IsConnected)
            {
                // _multiplexer.conn

            }
            Database = _multiplexer.GetDatabase();

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