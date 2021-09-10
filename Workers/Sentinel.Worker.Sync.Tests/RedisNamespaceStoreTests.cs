using StackExchange.Redis;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Worker.Sync.Tests
{
    public class RedisNamespaceStoreTests
    {
        private ITestOutputHelper output;
        public RedisNamespaceStoreTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public void GetDatabase(string constring)
        {
            var multi = ConnectionMultiplexer.Connect(constring);
            RedisNamespaceStore store = new RedisNamespaceStore(multi);
            output.WriteLine(store.Status());
        }
    }
}