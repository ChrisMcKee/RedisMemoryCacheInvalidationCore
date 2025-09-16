using StackExchange.Redis;

namespace RedisMemoryCacheInvalidation.Redis
{
    /// <summary>
    /// Factory class for creating Redis connection instances.
    /// </summary>
    internal static class RedisConnectionFactory
    {
        /// <summary>
        /// Creates a new Redis connection using an existing connection multiplexer.
        /// </summary>
        /// <param name="mux">The existing Redis connection multiplexer.</param>
        /// <returns>A Redis connection instance.</returns>
        public static IRedisConnection New(IConnectionMultiplexer mux)
        {
            return new ExistingRedisConnection(mux);
        }

        /// <summary>
        /// Creates a new standalone Redis connection using a configuration string.
        /// </summary>
        /// <param name="options">The Redis configuration string.</param>
        /// <returns>A Redis connection instance.</returns>
        public static IRedisConnection New(string options)
        {
            return new StandaloneRedisConnection(options);
        }
    }
}
