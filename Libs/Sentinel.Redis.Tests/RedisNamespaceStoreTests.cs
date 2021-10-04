using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Sentinel.Models.K8sDTOs;
using Sentinel.Redis;
using Sentinel.Tests.Helpers;
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

        [Fact]
        public void GetDatabase()
        {
            var multi = RedisExtensions.GetRedisMultiplexer();
            var store = new RedisNamespaceStore<TestClass>(multi, "test:*");
            output.WriteLine(store.Status());
        }


        [Fact]
        public async Task GetAsyncEnumerable()
        {
            var multi = RedisExtensions.GetRedisMultiplexer();
            var store = new RedisNamespaceStore<TestClass>(multi, "test:*");
            var items = store.GetAsyncEnumerable();
            await foreach (var item in items)
            {
                output.WriteLine(item.Name);
            }
        }

        [Fact]
        public async Task GetSet()
        {
            var multi = RedisExtensions.GetRedisMultiplexer();
            var store = new RedisNamespaceStore<TestClass>(multi, "test:*");
            output.WriteLine(store.Status());

            var tt = new TestClass("Name_test", "test:id");
            var q = await store.SetAsync("test:runs", tt);
            Assert.True(q);
            var ttt = await store.GetAsync("test:runs");
            Assert.NotNull(ttt);
        }

        [Fact]
        public async Task GetSetAsync()
        {
            var multi = RedisExtensions.GetRedisMultiplexer();
            var store = new RedisNamespaceStore<TestClass>(multi, "test:*");
            output.WriteLine(store.Status());

            var tt = new TestClass("Name_test", "id_test");

            var setTask = await store.SetAsync("test:runsAsync", tt);

            var getTask = await store.GetAsync("test:runs");

        }

        [Fact]
        public async Task GetListAsync()
        {
            var multi = RedisExtensions.GetRedisMultiplexer();
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