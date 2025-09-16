using System.Runtime.Caching;
using System.Threading.Tasks;
using RedisMemoryCacheInvalidation.Redis;

namespace RedisMemoryCacheInvalidation.Core
{
    /// <summary>
    /// Defines the contract for a Redis notification bus that handles cache invalidation messages.
    /// </summary>
    public interface IRedisNotificationBus
    {
        /// <summary>
        /// Gets the Redis connection used by this notification bus.
        /// </summary>
        IRedisConnection Connection { get; }

        /// <summary>
        /// Gets the invalidation strategy used by this notification bus.
        /// </summary>
        InvalidationStrategyType InvalidationStrategy { get; }

        /// <summary>
        /// Gets the local memory cache associated with this notification bus.
        /// </summary>
        MemoryCache LocalCache { get; }

        /// <summary>
        /// Gets the notification manager used for managing subscriptions.
        /// </summary>
        INotificationManager<string> Notifier { get; }

        /// <summary>
        /// Publishes an invalidation message for the specified key.
        /// </summary>
        /// <param name="key">The cache key to invalidate.</param>
        /// <returns>A task that represents the asynchronous operation and returns the number of subscribers that received the message.</returns>
        Task<long> NotifyAsync(string key);

        /// <summary>
        /// Starts the notification bus and establishes Redis connections.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the notification bus and disconnects from Redis.
        /// </summary>
        void Stop();
    }
}
