using Newtonsoft.Json;
using StackExchange.Redis;

namespace Sentinel.Worker.Sync
{
    public class RedisNamespaceStore
    {

        private readonly IConnectionMultiplexer _multiplexer;
        public RedisNamespaceStore(IConnectionMultiplexer multiplexer)
        {

            
            _multiplexer = multiplexer;
            if (!_multiplexer.IsConnected)
            {
                // _multiplexer.conn

            }
            var db = _multiplexer.GetDatabase();
            // JsonConvert.DeserializeObject()
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