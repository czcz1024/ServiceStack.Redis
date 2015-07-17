namespace TestRedisConnection
{
    using System.Collections.Generic;

    using ServiceStack.Redis;

    public class Incr
    {
        public long Id { get; set; }
    }

    public class IncrResponse
    {
        public long Result { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            //new LongRunningRedisPubSubServer().Execute("10.0.0.9");

            //new HashStressTest().Execute("127.0.0.1");
            //new HashStressTest().Execute("10.0.0.9");

            var client = new RedisClient("localhost");
            var list = new List<TestM>() {
                new TestM {Id=1,Name="a"},
                new TestM {Id=2,Name="b"},
                new TestM {Id=3,Name="c"},
            };

            client.As<TestM>().StoreAll(list);
            //client.Set("key1", 0);
            //client.Set("key2", false);

            
        }
    }

    public class TestM
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
