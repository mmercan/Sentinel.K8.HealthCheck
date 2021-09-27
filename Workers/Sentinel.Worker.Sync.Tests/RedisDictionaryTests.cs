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
            var multi = ConnectionMultiplexer.Connect(constring);
            var dic = new RedisDictionary<string, TestClass>(multi, "tests");
        }



        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public void Add(string constring)
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");


            var multi = ConnectionMultiplexer.Connect(constring);
            var dic = new RedisDictionary<string, TestClass>(multi, "tests");

            dic.Add(t1.Id, t1);
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


            var multi = ConnectionMultiplexer.Connect(constring);
            var dic = new RedisDictionary<string, TestClass>(multi, "tests");

            dic.Add(t1.Id, t1);
            var contain = dic.ContainsKey(t1.Id);
            Assert.True(contain);
            var removed = dic.Remove(t1.Id);
            Assert.True(removed);

        }


        [Theory]
        [InlineData("52.247.72.240:6379,defaultDatabase=2,password=2jWa8sSM8ZuhS3Qc")]
        public void TryGetValue(string constring)
        {
            var t1 = new TestClass("Name_T1", "test:id1");
            var t2 = new TestClass("Name_T2", "test:id2");
            var t3 = new TestClass("Name_T3", "test:id3");


            var multi = ConnectionMultiplexer.Connect(constring);
            var dic = new RedisDictionary<string, TestClass>(multi, "tests");

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


            var multi = ConnectionMultiplexer.Connect(constring);
            var dic = new RedisDictionary<string, TestClass>(multi, "tests");

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


            var multi = ConnectionMultiplexer.Connect(constring);
            var dic = new RedisDictionary<string, TestClass>(multi, "tests");

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


            var multi = ConnectionMultiplexer.Connect(constring);
            var dic = new RedisDictionary<string, TestClass>(multi, "tests");

            dic.Add(t1.Id, t1);
            var contain = dic.ContainsKey(t1.Id);
            Assert.True(contain);
            var val = dic[t1.Id];

            Assert.NotNull(val);
        }
    }
}