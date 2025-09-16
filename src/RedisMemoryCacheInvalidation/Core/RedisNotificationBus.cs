using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using RedisMemoryCacheInvalidation.Redis;
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
            Connection = RedisConnectionFactory.New(redisConfiguration);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RedisNotificationBus"/> class with an existing connection multiplexer.
        /// </summary>
        /// <param name="mux">The existing Redis connection multiplexer.</param>
        /// <param name="settings">The invalidation settings.</param>
        public RedisNotificationBus(ConnectionMultiplexer mux, InvalidationSettings settings)
            : this(settings)
        {
            Connection = RedisConnectionFactory.New(mux);
        }

        /// <summary>
        /// Starts the notification bus and establishes Redis connections.
        /// </summary>
        public void Start()
        {
            Connection.Connect();
            Connection.Subscribe(Constants.DefaultInvalidationChannel, OnInvalidationMessage);
            if(EnableKeySpaceNotifications)
                Connection.Subscribe(Constants.DefaultKeyspaceChannel, OnKeySpaceNotificationMessage);
        }

        /// <summary>
        /// Stops the notification bus and disconnects from Redis.
        /// </summary>
        public void Stop()
        {
            Connection.Disconnect();
        }

        /// <summary>
        /// Publishes an invalidation message for the specified key.
        /// </summary>
        /// <param name="key">The cache key to invalidate.</param>
        /// <returns>A task that represents the asynchronous operation and returns the number of subscribers that received the message.</returns>
        public Task<long> NotifyAsync(string key)
        {
            return Connection.PublishAsync(Constants.DefaultInvalidationChannel, key);
        }

        private void OnInvalidationMessage(RedisChannel pattern, RedisValue data)
        {
            if(pattern == Constants.DefaultInvalidationChannel)
            {
                ProcessInvalidationMessage(data.ToString());
            }
        }

        private void OnKeySpaceNotificationMessage(RedisChannel pattern, RedisValue data)
        {
            const string keyEventPrefix = "__keyevent";
            var prefix = pattern.ToString().Substring(0, 10);
            switch(prefix)
            {
                case keyEventPrefix:
                    ProcessInvalidationMessage(data.ToString());
                    break;
                default:
                    //nop
                    break;
            }
        }

        private void ProcessInvalidationMessage(string key)
        {
            if((InvalidationStrategy & InvalidationStrategyType.ChangeMonitor) == InvalidationStrategyType.ChangeMonitor)
            {
                Notifier.Notify(key);
            }

            if((InvalidationStrategy & InvalidationStrategyType.AutoCacheRemoval) == InvalidationStrategyType.AutoCacheRemoval && LocalCache != null)
            {
                LocalCache.Remove(key);
            }

            if((InvalidationStrategy & InvalidationStrategyType.External) == InvalidationStrategyType.External && NotificationCallback != null)
            {
                NotificationCallback?.Invoke(key);
            }
        }
    }
}
