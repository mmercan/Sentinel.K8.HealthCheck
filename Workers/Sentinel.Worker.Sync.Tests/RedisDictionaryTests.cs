using System.Collections.Generic;
using System.Threading.Tasks;
using Sentinel.Worker.Sync.RedisHelpers;
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

        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public void GetDatabase(string constring)
        {
            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = ConnectionMultiplexer.Connect(constring);
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");
        }



        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public void Add(string constring)
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = ConnectionMultiplexer.Connect(constring);
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");

            dic.Add(t1);
            var contain = dic.ContainsKey(t1.Id);
            Assert.True(contain);

        }


        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public async Task AddAsync(string constring)
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = ConnectionMultiplexer.Connect(constring);
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");

            await dic.AddAsync(t1);
            var contain = dic.ContainsKey(t1.Id);
            Assert.True(contain);

        }

        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public void Remove(string constring)
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = ConnectionMultiplexer.Connect(constring);
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");

            dic.Add(t1.Id, t1);
            var contain = dic.ContainsKey(t1.Id);
            Assert.True(contain);
            var removed = dic.Remove(t1);
            Assert.True(removed);

        }


        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public void TryGetValue(string constring)
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = ConnectionMultiplexer.Connect(constring);
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");

            dic.Add(t1.Id, t1);
            var contain = dic.ContainsKey(t1.Id);
            Assert.True(contain);
            TestClass tout;
            var valuegot = dic.TryGetValue(t1.Id, out tout);
            Assert.True(valuegot);

        }


        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public void Values(string constring)
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = ConnectionMultiplexer.Connect(constring);
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");

            dic.Add(t1.Id, t1);
            var contain = dic.ContainsKey(t1.Id);
            Assert.True(contain);
            var vals = dic.Values;

            Assert.NotEmpty(vals);
        }


        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public void Keys(string constring)
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = ConnectionMultiplexer.Connect(constring);
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");

            dic.Add(t1.Id, t1);

            var vals = dic.Keys;
            Assert.NotEmpty(vals);
        }


        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public void Key(string constring)
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = ConnectionMultiplexer.Connect(constring);
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");

            dic.Add(t1.Id, t1);
            var contain = dic.ContainsKey(t1);
            Assert.True(contain);
            var val = dic[t1.Id];

            Assert.NotNull(val);
        }


        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public void GetEnumerator(string constring)
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");

            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = ConnectionMultiplexer.Connect(constring);
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");

            dic.Add(t1.Id, t1);

            foreach (var item in dic)
            {
                output.WriteLine(item.Value.Name);
            }
            var val = dic[t1.Id];

            Assert.NotNull(val);
        }


        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public void AddMultiple(string constring)
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");
            List<TestClass> list = new List<TestClass> { t1, t2, t3 };


            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = ConnectionMultiplexer.Connect(constring);
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");

            dic.AddMultiple(list);
            var val = dic[t1.Id];

            Assert.NotNull(val);
        }


        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public void Clear(string constring)
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");
            List<TestClass> list = new List<TestClass> { t1, t2, t3 };


            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = ConnectionMultiplexer.Connect(constring);
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");

            dic.Clear();
            var isreadonly = dic.IsReadOnly;
        }



        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public void Sync(string constring)
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");
            List<TestClass> list1 = new List<TestClass> { t1, t2 };

            List<TestClass> list2 = new List<TestClass> { t2, t3 };


            var logger = Sentinel.Tests.Helpers.Helpers.GetLogger<RedisDictionary<string, TestClass>>();
            var multi = ConnectionMultiplexer.Connect(constring);
            var dic = new RedisDictionary<string, TestClass>(multi, logger, "tests");

            dic.AddMultiple(list1);
            dic.Sync(list2);

            Assert.Equal(dic.Count, 2);

        }
    }
}