using Microsoft.Extensions.Logging;
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
        /// <param name="enableResilience">Whether to wrap the connection with resilience features.</param>
        /// <param name="logger">Optional logger for diagnostic information.</param>
        /// <param name="healthCheckInterval">The interval between health checks in milliseconds.</param>
        /// <returns>A Redis connection instance.</returns>
        public static IRedisConnection New(IConnectionMultiplexer mux, bool enableResilience = false, ILogger logger = null, int healthCheckInterval = 30000)
        {
            var connection = new ExistingRedisConnection(mux, logger);
            return enableResilience ? new ResilientRedisConnection(connection, healthCheckInterval, logger) : (IRedisConnection) connection;
        }

        /// <summary>
        /// Creates a new standalone Redis connection using a configuration string.
        /// </summary>
        /// <param name="options">The Redis configuration string.</param>
        /// <param name="enableResilience">Whether to wrap the connection with resilience features.</param>
        /// <param name="logger">Optional logger for diagnostic information.</param>
        /// <param name="healthCheckInterval">The interval between health checks in milliseconds.</param>
        /// <returns>A Redis connection instance.</returns>
        public static IRedisConnection New(string options, bool enableResilience = false, ILogger logger = null, int healthCheckInterval = 30000)
        {
            var connection = new StandaloneRedisConnection(options, logger);
            return enableResilience ? new ResilientRedisConnection(connection, healthCheckInterval, logger) : (IRedisConnection) connection;
        }
    }
}
