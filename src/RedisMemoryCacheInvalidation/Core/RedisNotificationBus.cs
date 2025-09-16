using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using RedisMemoryCacheInvalidation.Redis;
using RedisMemoryCacheInvalidation.Utils;
using StackExchange.Redis;

namespace RedisMemoryCacheInvalidation.Core
{
    /// <summary>
    /// Invalidation message bus
    /// </summary>
    internal class RedisNotificationBus : IRedisNotificationBus
    {
        private readonly InvalidationSettings _settings;
        public INotificationManager<string> Notifier { get; private set; }
        public IRedisConnection Connection { get; internal set; }

        public InvalidationStrategyType InvalidationStrategy => _settings.InvalidationStrategy;
        public bool EnableKeySpaceNotifications => _settings.EnableKeySpaceNotifications;
        public MemoryCache LocalCache => _settings.TargetCache;
        public Action<string> NotificationCallback => _settings.InvalidationCallback;

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisNotificationBus"/> class.
        /// </summary>
        /// <param name="settings">The invalidation settings.</param>
        private RedisNotificationBus(InvalidationSettings settings)
        {
            _settings = settings;
            Notifier = new NotificationManager();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisNotificationBus"/> class with a Redis configuration string.
        /// </summary>
        /// <param name="redisConfiguration">The Redis configuration string.</param>
        /// <param name="settings">The invalidation settings.</param>
        public RedisNotificationBus(string redisConfiguration, InvalidationSettings settings)
            : this(settings)
        {
            Connection = RedisConnectionFactory.New(redisConfiguration, settings.EnableResilience, settings.Logger, settings.HealthCheckIntervalMs);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisNotificationBus"/> class with an existing connection multiplexer.
        /// </summary>
        /// <param name="mux">The existing Redis connection multiplexer.</param>
        /// <param name="settings">The invalidation settings.</param>
        public RedisNotificationBus(ConnectionMultiplexer mux, InvalidationSettings settings)
            : this(settings)
        {
            Connection = RedisConnectionFactory.New(mux, settings.EnableResilience, settings.Logger, settings.HealthCheckIntervalMs);
        }

        /// <summary>
        /// Starts the notification bus and establishes Redis connections.
        /// </summary>
        public void Start()
        {
            SafeLogger.LogInformation(_settings.Logger, "Starting Redis notification bus");

            if(!Connection.Connect())
            {
                SafeLogger.LogError(_settings.Logger, "Failed to establish Redis connection");
                return;
            }

            SafeLogger.LogInformation(_settings.Logger, "Successfully connected to Redis");

            Connection.Subscribe(Constants.DefaultInvalidationChannel, OnInvalidationMessage);
            SafeLogger.LogTrace(_settings.Logger, "Subscribed to invalidation channel: {Channel}", Constants.DefaultInvalidationChannel);

            if(EnableKeySpaceNotifications)
            {
                Connection.Subscribe(Constants.DefaultKeyspaceChannel, OnKeySpaceNotificationMessage);
                SafeLogger.LogTrace(_settings.Logger, "Subscribed to keyspace notifications channel: {Channel}", Constants.DefaultKeyspaceChannel);
            }

            SafeLogger.LogInformation(_settings.Logger, "Redis notification bus started successfully");
        }

        /// <summary>
        /// Stops the notification bus and disconnects from Redis.
        /// </summary>
        public void Stop()
        {
            SafeLogger.LogInformation(_settings.Logger, "Stopping Redis notification bus");
            Connection.Disconnect();
            SafeLogger.LogInformation(_settings.Logger, "Redis notification bus stopped");
        }

        /// <summary>
        /// Publishes an invalidation message for the specified key.
        /// </summary>
        /// <param name="key">The cache key to invalidate.</param>
        /// <returns>A task that represents the asynchronous operation and returns the number of subscribers that received the message.</returns>
        public Task<long> NotifyAsync(string key)
        {
            SafeLogger.LogTrace(_settings.Logger, "Publishing invalidation message for key: {Key}", key);

            if (_settings.EnableResilience)
            {
                return RetryHelper.ExecuteWithRetryAsync(
                    () => Connection.PublishAsync(Constants.DefaultInvalidationChannel, key),
                    _settings.MaxRetryAttempts,
                    _settings.RetryDelayMs,
                    true,
                    0L,
                    _settings.Logger);
            }

            return Connection.PublishAsync(Constants.DefaultInvalidationChannel, key);
        }

        private void OnInvalidationMessage(RedisChannel pattern, RedisValue data)
        {
            if(pattern != Constants.DefaultInvalidationChannel)
            {
                return;
            }

            SafeLogger.LogTrace(_settings.Logger, "Received invalidation message on channel {Channel}: {Data}", pattern, data);
            ProcessInvalidationMessage(data.ToString());
        }

        private void OnKeySpaceNotificationMessage(RedisChannel pattern, RedisValue data)
        {
            const string keyEventPrefix = "__keyevent";
            var prefix = pattern.ToString().Substring(0, 10);
            switch(prefix)
            {
                case keyEventPrefix:
                    SafeLogger.LogTrace(_settings.Logger, "Received keyspace notification on channel {Channel}: {Data}", pattern, data);
                    ProcessInvalidationMessage(data.ToString());
                    break;
                default:
                    //nop
                    break;
            }
        }

        private void ProcessInvalidationMessage(string key)
        {
            SafeLogger.LogTrace(_settings.Logger, "Processing invalidation message for key: {Key}, Strategy: {Strategy}", key, InvalidationStrategy);

            try
            {
                if((InvalidationStrategy & InvalidationStrategyType.ChangeMonitor) == InvalidationStrategyType.ChangeMonitor)
                {
                    SafeLogger.LogTrace(_settings.Logger, "Notifying change monitors for key: {Key}", key);
                    Notifier.Notify(key);
                }

                if((InvalidationStrategy & InvalidationStrategyType.AutoCacheRemoval) == InvalidationStrategyType.AutoCacheRemoval && LocalCache != null)
                {
                    SafeLogger.LogTrace(_settings.Logger, "Auto-removing from cache key: {Key}", key);
                    LocalCache.Remove(key);
                }

                if((InvalidationStrategy & InvalidationStrategyType.External) == InvalidationStrategyType.External && NotificationCallback != null)
                {
                    SafeLogger.LogTrace(_settings.Logger, "Invoking external callback for key: {Key}", key);
                    NotificationCallback?.Invoke(key);
                }

                SafeLogger.LogTrace(_settings.Logger, "Successfully processed invalidation message for key: {Key}", key);
            }
            catch(Exception ex)
            {
                SafeLogger.LogError(_settings.Logger, ex, "Failed to process invalidation message for key: {Key}", key);
                // Individual strategy failures shouldn't crash the entire system
            }
        }
    }
}
