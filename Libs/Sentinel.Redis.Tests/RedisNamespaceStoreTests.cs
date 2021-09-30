using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Sentinel.Models.K8sDTOs;
using Sentinel.Redis;
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
            var store = new RedisNamespaceStore<TestClass>(multi, "test:*");
            output.WriteLine(store.Status());
        }


        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public async Task GetAsyncEnumerable(string constring)
        {
            var multi = ConnectionMultiplexer.Connect(constring);
            var store = new RedisNamespaceStore<TestClass>(multi, "test:*");
            var items = store.GetAsyncEnumerable();
            await foreach (var item in items)
            {
                output.WriteLine(item.Name);
            }
        }

        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public async Task GetSet(string constring)
        {
            var multi = ConnectionMultiplexer.Connect(constring);
            var store = new RedisNamespaceStore<TestClass>(multi, "test:*");
            output.WriteLine(store.Status());

            var tt = new TestClass("Name_test", "test:id");
            var q = await store.SetAsync("test:runs", tt);
            Assert.True(q);
            var ttt = await store.GetAsync("test:runs");
            Assert.NotNull(ttt);
        }

        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public async Task GetSetAsync(string constring)
        {
            var multi = ConnectionMultiplexer.Connect(constring);
            var store = new RedisNamespaceStore<TestClass>(multi, "test:*");
            output.WriteLine(store.Status());

            var tt = new TestClass("Name_test", "id_test");

            var setTask = await store.SetAsync("test:runsAsync", tt);

            var getTask = await store.GetAsync("test:runs");

        }

        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public async Task GetListAsync(string constring)
        {
            var multi = ConnectionMultiplexer.Connect(constring);
            var store = new RedisNamespaceStore<TestClass>(multi, "test:*");
            output.WriteLine(store.Status());

            var tt = new TestClass("Name_test", "id_test");

            var setTask = await store.SetAsync("test:runsAsync", tt);

            var tests = await store.GetListAsync();
            Assert.NotEmpty(tests);

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

        [Key]
        public string Id { get; set; }
    }
}