using RedisMemoryCacheInvalidation.Utils;
using StackExchange.Redis;

namespace RedisMemoryCacheInvalidation.Redis
{
    /// <summary>
    /// A Redis connection that uses an existing connection multiplexer.
    /// </summary>
    internal class ExistingRedisConnection : RedisConnectionBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ExistingRedisConnection"/> class.
        /// </summary>
        /// <param name="mux">The existing Redis connection multiplexer.</param>
        public ExistingRedisConnection(IConnectionMultiplexer mux)
        {
            Multiplexer = mux;
        }

        /// <summary>
        /// Establishes a connection to Redis (returns current connection status since multiplexer is already provided).
        /// </summary>
        /// <returns>true if the connection is established; otherwise, false.</returns>
        public override bool Connect()
        {
            return IsConnected;
        }

        /// <summary>
        /// Disconnects from Redis and cleans up resources.
        /// </summary>
        public override void Disconnect()
        {
            UnsubscribeAll();
        }
    }
}
