using StackExchange.Redis;
using Xunit.Abstractions;
using Xunit;
using System.Threading.Tasks;
using System.Collections.Generic;
using Sentinel.Redis;

namespace Sentinel.Worker.Sync.Tests
{
    public class IDatabaseGenericExtensionTests
    {
        private ITestOutputHelper output;
        public IDatabaseGenericExtensionTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public void GetSet(string constring)
        {
            var multi = ConnectionMultiplexer.Connect(constring);
            var tt = new TestClass("Name_test", "test:id");
            var db = multi.GetDatabase();
            db.Set<TestClass>("test:id", tt);

            var ttdb = db.Get<TestClass>("test:id");
            Assert.Equal(tt.Name, ttdb.Name);

        }


        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public async Task GetSetAsync(string constring)
        {
            var multi = ConnectionMultiplexer.Connect(constring);
            var tt = new TestClass("Name_test", "test:id");
            var db = multi.GetDatabase();
            await db.SetAsync<TestClass>("test:id", tt);

            var ttdb = await db.GetAsync<TestClass>("test:id");
            Assert.Equal(tt.Name, ttdb.Name);

        }


        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public async Task SetListAsyncFromKeyAttr(string constring)
        {
            var multi = ConnectionMultiplexer.Connect(constring);
            var tt = new TestClass("Name_test", "test:id");
            List<TestClass> lists = new List<TestClass>();
            lists.Add(tt);
            var db = multi.GetDatabase();
            await db.SetListAsync<TestClass>(lists);
        }


        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public async Task SetListAsyncFromFunc(string constring)
        {
            var multi = ConnectionMultiplexer.Connect(constring);
            var tt = new TestClass("Name_test", "test:id");
            List<TestClass> lists = new List<TestClass>();
            lists.Add(tt);
            var db = multi.GetDatabase();
            await db.SetListAsync<TestClass>(lists, (t) => t.Id);
        }

    }
}