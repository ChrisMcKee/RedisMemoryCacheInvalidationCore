using System;
using System.Runtime.Caching;
using Microsoft.Extensions.Logging;

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
        /// Enables resilience features such as automatic reconnection and health monitoring.
        /// </summary>
        public bool EnableResilience { get; set; }

        /// <summary>
        /// The interval between health checks in milliseconds when resilience is enabled.
        /// </summary>
        public int HealthCheckIntervalMs { get; set; }

        /// <summary>
        /// The maximum number of retry attempts for failed operations.
        /// </summary>
        public int MaxRetryAttempts { get; set; }

        /// <summary>
        /// The base delay between retry attempts in milliseconds.
        /// </summary>
        public int RetryDelayMs { get; set; }

        /// <summary>
        /// Optional logger for diagnostic information and error logging.
        /// </summary>
        public ILogger Logger { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InvalidationSettings"/> class with default values.
        /// </summary>
        public InvalidationSettings()
        {
            InvalidationStrategy = InvalidationStrategyType.All;
            TargetCache = MemoryCache.Default;
            EnableKeySpaceNotifications = false;
            InvalidationCallback = null;
            EnableResilience = false;
            HealthCheckIntervalMs = 30000; // 30 seconds
            MaxRetryAttempts = 3;
            RetryDelayMs = 1000; // 1 second
            Logger = null;
        }
    }
}
