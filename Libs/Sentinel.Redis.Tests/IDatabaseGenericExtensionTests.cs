using StackExchange.Redis;
using Xunit.Abstractions;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sentinel.Redis;
using Sentinel.Tests.Helpers;

namespace Sentinel.Worker.Sync.Tests
{
    public class IDatabaseGenericExtensionTests
    {
        private ITestOutputHelper output;
        private IConnectionMultiplexer multi;
        public IDatabaseGenericExtensionTests(ITestOutputHelper output)
        {
            this.output = output;
            multi = RedisExtensions.GetRedisMultiplexer();
        }

        [Fact]
        public void GetSet()
        {

            var tt = new TestClass("Name_test", "test:id");
            var db = multi.GetDatabase();
            db.Set<TestClass>("test:id", tt);

            var ttdb = db.Get<TestClass>("test:id");
            Assert.Equal(tt.Name, ttdb?.Name);

        }

        [Fact]
        public async Task GetSetAsync()
        {

            var tt = new TestClass("Name_test", "test:id");
            var db = multi.GetDatabase();
            await db.SetAsync<TestClass>("test:id", tt);

            var ttdb = await db.GetAsync<TestClass>("test:id");
            Assert.Equal(tt.Name, ttdb?.Name);

        }


        [Fact]
        public async Task SetListAsyncFromKeyAttr()
        {

            var tt = new TestClass("Name_test", "test:id");
            List<TestClass> lists = new List<TestClass>();
            lists.Add(tt);
            var db = multi.GetDatabase();
            await db.SetListAsync<TestClass>(lists);
        }


        [Fact]
        public async Task SetListAsyncFromFunc()
        {

            var tt = new TestClass("Name_test", "test:id");
            List<TestClass> lists = new List<TestClass>();
            lists.Add(tt);
            var db = multi.GetDatabase();
            await db.SetListAsync<TestClass>(lists, (t) => t.Id);
        }

    }
}