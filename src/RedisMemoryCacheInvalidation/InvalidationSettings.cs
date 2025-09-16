using System;
using System.Runtime.Caching;

namespace RedisMemoryCacheInvalidation
{
    /// <summary>
    /// Configuration settings for Redis memory cache invalidation.
    /// </summary>
    public class InvalidationSettings
    {
        /// <summary>
        /// How to process invalidation message (remove from cache, invoke callback, notify change monitor, <see cref="InvalidationStrategyType"/>)
        /// </summary>
        public InvalidationStrategyType InvalidationStrategy { get; set; }

        /// <summary>
        /// Target MemoryCache when InvalidationStrategy=AutoCacheRemoval or All
        /// </summary>
        public MemoryCache TargetCache { get; set; }

        /// <summary>
        /// Subscribe to keyspace notification (if enabled on the redis DB)
        /// </summary>
        public bool EnableKeySpaceNotifications { get; set; }

        /// <summary>
        /// Invalidation callback invoked when InvalidationStrategy=External.
        /// </summary>
        public Action<string> InvalidationCallback { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidationSettings"/> class with default values.
        /// </summary>
        public InvalidationSettings()
        {
            InvalidationStrategy = InvalidationStrategyType.All;
            TargetCache = MemoryCache.Default;
            EnableKeySpaceNotifications = false;
            InvalidationCallback = null;
        }
    }
}
