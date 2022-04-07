using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace Sentinel.Tests.Helpers
{
    public class RedisExtensions
    {
        public static IConnectionMultiplexer GetRedisMultiplexer()
        {
            var config = new ConfigurationBuilder()
            .AddEnvironmentVariables()
            .Build();

            var redisconstring = config["RedisConnection"];
            if (string.IsNullOrWhiteSpace(redisconstring))
            {
                throw new Exception("RedisConnection is not set in the environment variables");
            }
            IConnectionMultiplexer rediscon = ConnectionMultiplexer.Connect(redisconstring);

            return rediscon;
        }
    }
}