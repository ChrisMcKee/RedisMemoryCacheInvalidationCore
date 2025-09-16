using RedisMemoryCacheInvalidation.Utils;
using StackExchange.Redis;

namespace RedisMemoryCacheInvalidation.Redis
{
    internal class ExistingRedisConnection : RedisConnectionBase
    {
        public ExistingRedisConnection(IConnectionMultiplexer mux)
        {
            Multiplexer = mux;
        }

        public override bool Connect()
        {
            return IsConnected;
        }

        public override void Disconnect()
        {
            UnsubscribeAll();
        }
    }
}
