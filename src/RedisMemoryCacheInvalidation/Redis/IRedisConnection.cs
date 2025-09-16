using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisMemoryCacheInvalidation.Redis
{
    /// <summary>
    /// Defines the contract for a Redis connection that handles pub/sub operations and configuration.
    /// </summary>
    public interface IRedisConnection
    {
        /// <summary>
        /// Gets a value indicating whether the connection is currently connected to Redis.
        /// </summary>
        bool IsConnected { get; }
        
        /// <summary>
        /// Establishes a connection to Redis.
        /// </summary>
        /// <returns>true if the connection was established successfully; otherwise, false.</returns>
        bool Connect();
        
        /// <summary>
        /// Disconnects from Redis and cleans up resources.
        /// </summary>
        void Disconnect();
        
        /// <summary>
        /// Gets the Redis server configuration asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation and returns the configuration key-value pairs.</returns>
        Task<KeyValuePair<string, string>[]> GetConfigAsync();
        
        /// <summary>
        /// Subscribes to a Redis channel and sets up a message handler.
        /// </summary>
        /// <param name="channel">The Redis channel to subscribe to.</param>
        /// <param name="handler">The action to invoke when a message is received on the channel.</param>
        void Subscribe(string channel, Action<RedisChannel, RedisValue> handler);
        
        /// <summary>
        /// Unsubscribes from all Redis channels.
        /// </summary>
        void UnsubscribeAll();
        
        /// <summary>
        /// Publishes a message to a Redis channel asynchronously.
        /// </summary>
        /// <param name="channel">The Redis channel to publish to.</param>
        /// <param name="value">The message value to publish.</param>
        /// <returns>A task that represents the asynchronous operation and returns the number of subscribers that received the message.</returns>
        Task<long> PublishAsync(string channel, string value);
    }
}
