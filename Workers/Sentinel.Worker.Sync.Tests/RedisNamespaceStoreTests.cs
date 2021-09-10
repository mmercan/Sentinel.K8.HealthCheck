using StackExchange.Redis;
using Xunit;
using Xunit.Abstractions;
using static Sentinel.Worker.Sync.RedisHelpers.IDatabaseGenericExtension;

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

        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public void GetSet(string constring)
        {
            var multi = ConnectionMultiplexer.Connect(constring);
            RedisNamespaceStore store = new RedisNamespaceStore(multi);
            output.WriteLine(store.Status());

            var tt = new TestClass("Name_test", "id_test");
            store.Database.Set<TestClass>("test:runs", tt);

            var ttt = store.Database.Get<TestClass>("test:runs");

        }

        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public void GetSetAsync(string constring)
        {
            var multi = ConnectionMultiplexer.Connect(constring);
            RedisNamespaceStore store = new RedisNamespaceStore(multi);
            output.WriteLine(store.Status());

            var tt = new TestClass("Name_test", "id_test");

            var setTask = store.Database.SetAsync<TestClass>("test:runsAsync", tt);
            setTask.Wait();
            var getTask = store.Database.GetAsync<TestClass>("test:runs");
            getTask.Wait();
        }


    }

    public class TestClass
    {
        public TestClass()
        {

        }
        public TestClass(string name, string id)
        {
            Name = name;
            Id = id;
        }
        public string Name { get; set; }
        public string Id { get; set; }
    }
}