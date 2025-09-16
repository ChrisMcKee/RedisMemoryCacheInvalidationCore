namespace RedisMemoryCacheInvalidation
{
    /// <summary>
    /// Contains constant values used throughout the Redis memory cache invalidation library.
    /// </summary>
    internal static class Constants
    {
        /// <summary>
        /// The default Redis channel used for invalidation messages.
        /// </summary>
        public const string DefaultInvalidationChannel = "invalidate";

        /// <summary>
        /// The default Redis channel pattern used for keyspace notifications.
        /// </summary>
        public const string DefaultKeyspaceChannel = "__keyevent*__:*";
    }
}
