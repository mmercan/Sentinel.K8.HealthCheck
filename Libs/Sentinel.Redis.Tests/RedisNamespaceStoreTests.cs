using System;
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
        private IConnectionMultiplexer multi;
        private RedisNamespaceStore<TestClass> store;
        public RedisNamespaceStoreTests(ITestOutputHelper output)
        {
            this.output = output;
            multi = RedisExtensions.GetRedisMultiplexer();
            store = new RedisNamespaceStore<TestClass>(multi, "test:*");
        }

        [Fact]
        public void GetDatabase()
        {
            output.WriteLine(store.Status());
        }


        [Fact]
        public async Task GetAsyncEnumerable()
        {
            var items = store.GetAsyncEnumerable();
            await foreach (var item in items)
            {
                output.WriteLine(item?.Name);
            }
        }

        [Fact]
        public async Task GetSet()
        {
            output.WriteLine(store.Status());

            var tt = new TestClass("Name_test", "test:id");
            var q = await store.SetAsync("test:runs", tt);
            Assert.True(q);
            var ttt = await store.GetAsync("test:runs");
            Assert.NotNull(ttt);

            var dd = Guid.NewGuid().ToString();
            var tttt = await store.GetAsync(dd);
            Assert.Null(tttt);

            var items = store.GetAsyncEnumerable(dd);
            //Assert.Null(items);
        }

        [Fact]
        public async Task GetSetAsync()
        {

            output.WriteLine(store.Status());

            var tt = new TestClass("Name_test", "id_test");

            var setTask = await store.SetAsync("test:runsAsync", tt);

            var getTask = await store.GetAsync("test:runs");

        }

        [Fact]
        public async Task GetListAsync()
        {
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
        public string Name { get; set; } = default!;

        [Key]
        public string Id { get; set; } = default!;
    }
}