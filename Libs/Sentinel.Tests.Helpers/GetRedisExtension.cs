using StackExchange.Redis;

namespace Sentinel.Tests.Helpers
{
    public class RedisExtensions
    {
        public static IConnectionMultiplexer GetRedisMultiplexer()
        {
            IConnectionMultiplexer rediscon = ConnectionMultiplexer.Connect("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc");
            return rediscon;
        }
    }
}