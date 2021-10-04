using System.Collections.Generic;
using System.Threading.Tasks;
using Sentinel.Redis;
using Sentinel.Tests.Helpers;
using StackExchange.Redis;
using Xunit;
using Xunit.Abstractions;

namespace Sentinel.Worker.Sync.Tests
{
    public class RedisDictionaryTests
    {
        private ITestOutputHelper output;
        public RedisDictionaryTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void GetDatabase()
        {
            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = RedisExtensions.GetRedisMultiplexer();
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");
        }



        [Fact]
        public void Add()
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = RedisExtensions.GetRedisMultiplexer();
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");

            dic.Add(t1);
            var contain = dic.ContainsKey(t1.Id);
            Assert.True(contain);

        }


        [Fact]
        public async Task AddAsync()
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = RedisExtensions.GetRedisMultiplexer();
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");

            await dic.AddAsync(t1);
            var contain = dic.ContainsKey(t1.Id);
            Assert.True(contain);

        }

        [Fact]
        public void Remove()
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = RedisExtensions.GetRedisMultiplexer();
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");

            dic.Add(t1.Id, t1);
            var contain = dic.ContainsKey(t1.Id);
            Assert.True(contain);
            var removed = dic.Remove(t1);
            Assert.True(removed);

        }


        [Fact]
        public void TryGetValue()
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = RedisExtensions.GetRedisMultiplexer();
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");

            dic.Add(t1.Id, t1);
            var contain = dic.ContainsKey(t1.Id);
            Assert.True(contain);
            TestClass tout;
            var valuegot = dic.TryGetValue(t1.Id, out tout);
            Assert.True(valuegot);

        }


        [Fact]
        public void Values()
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = RedisExtensions.GetRedisMultiplexer();
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");

            dic.Add(t1.Id, t1);
            var contain = dic.ContainsKey(t1.Id);
            Assert.True(contain);
            var vals = dic.Values;

            Assert.NotEmpty(vals);
        }


        [Fact]
        public void Keys()
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = RedisExtensions.GetRedisMultiplexer();
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");

            dic.Add(t1.Id, t1);

            var vals = dic.Keys;
            Assert.NotEmpty(vals);
        }


        [Fact]
        public void Key()
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = RedisExtensions.GetRedisMultiplexer();
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");

            dic.Add(t1.Id, t1);
            var contain = dic.ContainsKey(t1);
            Assert.True(contain);
            var val = dic[t1.Id];

            Assert.NotNull(val);
        }


        [Fact]
        public void GetEnumerator()
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = RedisExtensions.GetRedisMultiplexer();
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");

            dic.Add(t1.Id, t1);

            foreach (var item in dic)
            {
                output.WriteLine(item.Value.Name);
            }
            var val = dic[t1.Id];

            Assert.NotNull(val);
        }


        [Fact]
        public void AddMultiple()
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");
            List<TestClass> list = new List<TestClass> { t1, t2, t3 };


            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = RedisExtensions.GetRedisMultiplexer();
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");

            dic.AddMultiple(list);
            var val = dic[t1.Id];

            Assert.NotNull(val);
        }


        [Fact]
        public void Clear()
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");
            List<TestClass> list = new List<TestClass> { t1, t2, t3 };


            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = RedisExtensions.GetRedisMultiplexer();
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");

            dic.Clear();
            var isreadonly = dic.IsReadOnly;
        }



        [Fact]
        public void Sync()
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");
            List<TestClass> list1 = new List<TestClass> { t1, t2 };

            List<TestClass> list2 = new List<TestClass> { t2, t3 };


            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = RedisExtensions.GetRedisMultiplexer();
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");

            dic.AddMultiple(list1);
            dic.Sync(list2);

            Assert.Equal(dic.Count, 2);

        }
    }
}