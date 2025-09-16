using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RedisMemoryCacheInvalidation.Utils;
using StackExchange.Redis;

namespace RedisMemoryCacheInvalidation.Redis
{
    internal abstract class RedisConnectionBase : IRedisConnection
    {
        protected IConnectionMultiplexer Multiplexer;

        /// <summary>
        /// Gets a value indicating whether the connection is currently connected to Redis.
        /// </summary>
        public bool IsConnected
        {
            get { return Multiplexer != null && Multiplexer.IsConnected; }
        }

        /// <summary>
        /// Subscribes to a Redis channel and sets up a message handler.
        /// </summary>
        /// <param name="channel">The Redis channel to subscribe to.</param>
        /// <param name="handler">The action to invoke when a message is received on the channel.</param>
        public void Subscribe(string channel, Action<RedisChannel, RedisValue> handler)
        {
            if(IsConnected)
            {
                var subscriber = Multiplexer.GetSubscriber();
                subscriber.Subscribe(RedisChannel.Literal(channel), handler);
            }
        }

        /// <summary>
        /// Unsubscribes from all Redis channels.
        /// </summary>
        public void UnsubscribeAll()
        {
            if(!IsConnected)
            {
                return;
            }

            Multiplexer.GetSubscriber().UnsubscribeAll();
        }

        /// <summary>
        /// Publishes a message to a Redis channel asynchronously.
        /// </summary>
        /// <param name="channel">The Redis channel to publish to.</param>
        /// <param name="value">The message value to publish.</param>
        /// <returns>A task that represents the asynchronous operation and returns the number of subscribers that received the message.</returns>
        public Task<long> PublishAsync(string channel, string value)
        {
            if(!IsConnected)
            {
                return TaskCache.FromResult(0L);
            }

            return Multiplexer.GetSubscriber().PublishAsync(RedisChannel.Literal(channel), value);
        }

        /// <summary>
        /// Gets the Redis server configuration asynchronously.
        /// </summary>
        /// <returns>A task that represents the asynchronous operation and returns the configuration key-value pairs.</returns>
        public Task<KeyValuePair<string, string>[]> GetConfigAsync()
        {
            if(!IsConnected)
            {
                return TaskCache.FromResult(new KeyValuePair<string, string>[] { });
            }

            var server = GetServer();
            return server.ConfigGetAsync();
        }

        /// <summary>
        /// Gets the Redis server instance from the connection multiplexer.
        /// </summary>
        /// <returns>The Redis server instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown when no master endpoint is found or multiple master endpoints are found.</exception>
        protected IServer GetServer()
        {
            var endpoints = Multiplexer.GetEndPoints();
            IServer result = null;
            foreach(var endpoint in endpoints)
            {
                var server = Multiplexer.GetServer(endpoint);
                if(server.IsReplica || !server.IsConnected) continue;
                if(result != null)
                {
                    throw new InvalidOperationException("Requires exactly one master endpoint (found " + server.EndPoint + " and " + result?.EndPoint + ")");
                }
                result = server;
            }

            if(result == null)
            {
                throw new InvalidOperationException("Requires exactly one master endpoint (found none)");
            }
            return result;
        }

        /// <summary>
        /// Establishes a connection to Redis.
        /// </summary>
        /// <returns>true if the connection was established successfully; otherwise, false.</returns>
        public abstract bool Connect();

        /// <summary>
        /// Disconnects from Redis and cleans up resources.
        /// </summary>
        public abstract void Disconnect();
    }
}
