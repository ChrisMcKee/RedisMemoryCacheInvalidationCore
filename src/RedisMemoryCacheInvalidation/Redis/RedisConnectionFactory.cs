using StackExchange.Redis;

namespace RedisMemoryCacheInvalidation.Redis
{
    internal class RedisConnectionFactory
    {
        public static IRedisConnection New(IConnectionMultiplexer mux)
        {
            return new ExistingRedisConnection(mux);
        }

        public static IRedisConnection New(string options)
        {
            return new StandaloneRedisConnection(options);
        }
    }
}
