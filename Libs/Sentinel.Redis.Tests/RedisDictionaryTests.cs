using System.Collections.Generic;
using System.Linq;
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
        private IConnectionMultiplexer multi;
        public RedisDictionaryTests(ITestOutputHelper output)
        {
            this.output = output;
            var multi = RedisExtensions.GetRedisMultiplexer();
        }

        [Fact]
        public void GetDatabase()
        {
            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<TestClass>>();

            var dic = new RedisDictionary<TestClass>(multi, logger, "tests-1");
        }



        [Fact]
        public void Add()
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<TestClass>>();
            var dic = new RedisDictionary<TestClass>(multi, logger, "tests-2");

            dic.Add(t1);
            var contain = dic.ContainsKey(t1.Id);
            Assert.True(contain);

        }


        [Fact]
        public void SetByIndex()
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<TestClass>>();
            var dic = new RedisDictionary<TestClass>(multi, logger, "tests-2");

            dic["test:id1"] = t3;
            var contain = dic.ContainsKey(t1.Id);
            Assert.True(contain);

        }


        [Fact]
        public async Task AddAsync()
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<TestClass>>();
            var dic = new RedisDictionary<TestClass>(multi, logger, "tests-3");

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

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<TestClass>>();
            var dic = new RedisDictionary<TestClass>(multi, logger, "tests-4");

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

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<TestClass>>();
            var dic = new RedisDictionary<TestClass>(multi, logger, "tests-5");

            dic.Add(t1.Id, t1);
            var contain = dic.ContainsKey(t1.Id);
            Assert.True(contain);
            TestClass tout;
            var valuegot = dic.TryGetValue(t1.Id, out tout);
            Assert.True(valuegot);

            var valuegotnotexits = dic.TryGetValue(t3.Id, out tout);

        }


        [Fact]
        public void Values()
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<TestClass>>();
            var dic = new RedisDictionary<TestClass>(multi, logger, "tests-6");

            dic.Add(t1.Id, t1);
            var contain = dic.ContainsKey(t1.Id);
            Assert.True(contain);
            var vals = dic.Values;

            Assert.NotEmpty(vals);
        }


        [Fact]
        public void Contains()
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<TestClass>>();
            var dic = new RedisDictionary<TestClass>(multi, logger, "tests-6");

            dic.Add(t1.Id, t1);
            var contain = dic.Contains(new KeyValuePair<string, TestClass>(t1.Id, t1));
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
            List<TestClass> list = new List<TestClass> { t1, t2, t3 };

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<TestClass>>();
            var dic = new RedisDictionary<TestClass>(multi, logger, "tests-7");

            dic.AddMultiple(list);

            var vals = dic.Keys;
            Assert.NotEmpty(vals);
        }


        [Fact]
        public void Key()
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<TestClass>>();
            var dic = new RedisDictionary<TestClass>(multi, logger, "tests-8");

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

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<TestClass>>();
            var dic = new RedisDictionary<TestClass>(multi, logger, "tests-9");

            dic.Add(t1.Id.ToJSON(), t1);

            foreach (var item in dic)
            {
                output.WriteLine(item.Value.Name);
            }
            var val = dic[t1.Id].ToJSON();
            var enuma = dic.GetEnumerator();
            Assert.NotNull(val);
        }


        [Fact]
        public void AddMultiple()
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");
            List<TestClass> list = new List<TestClass> { t1, t2, t3 };

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<TestClass>>();
            var dic = new RedisDictionary<TestClass>(multi, logger, "tests-10");

            dic.AddMultiple(list);
            var val = dic[t1.Id];

            Assert.NotNull(val);
        }


        [Fact]
        public void AddMultipleKeyValuePair()
        {
            var t1 = new KeyValuePair<string, TestClass>("Name_T1", new TestClass("Name_T1", "test:id1"));
            var t2 = new KeyValuePair<string, TestClass>("Name_T2", new TestClass("Name_T2", "test:id2"));
            var t3 = new KeyValuePair<string, TestClass>("Name_T3", new TestClass("Name_T3", "test:id3"));

            var list = new List<KeyValuePair<string, TestClass>> { t1, t2, t3 };

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<TestClass>>();
            var dic = new RedisDictionary<TestClass>(multi, logger, "tests-10");

            dic.AddMultiple(list);
            var val = dic[t1.Key];

            Assert.NotNull(val);
        }


        [Fact]
        public void Clear()
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");
            List<TestClass> list = new List<TestClass> { t1, t2, t3 };

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<TestClass>>();
            var dic = new RedisDictionary<TestClass>(multi, logger, "tests-11");

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

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<TestClass>>();
            var dic = new RedisDictionary<TestClass>(multi, logger, "tests-12");

            dic.AddMultiple(list1);
            dic.Sync(list2);

            Assert.Equal(dic.Count, 2);

        }


        [Fact]
        public void SyncFunc()
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");
            List<TestClass> list1 = new List<TestClass> { t1, t2 };

            List<TestClass> list2 = new List<TestClass> { t2, t3 };

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<TestClass>>();
            var dic = new RedisDictionary<TestClass>(multi, logger, "tests-12");

            dic.AddMultiple(list1);
            dic.Sync(list2, p => p.Id);

            Assert.Equal(dic.Count, 2);
        }
    }
}