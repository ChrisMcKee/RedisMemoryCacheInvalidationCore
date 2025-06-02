using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RedisMemoryCacheInvalidation.Utils;
using StackExchange.Redis;

namespace RedisMemoryCacheInvalidation.Redis
{
    internal abstract class RedisConnectionBase : IRedisConnection
    {
        protected IConnectionMultiplexer multiplexer;

        public bool IsConnected
        {
            get { return multiplexer != null && multiplexer.IsConnected; }
        }

        public void Subscribe(string channel, Action<RedisChannel, RedisValue> handler)
        {
            if(IsConnected)
            {
                var subscriber = multiplexer.GetSubscriber();
                subscriber.Subscribe(RedisChannel.Literal(channel), handler);
            }
        }

        public void UnsubscribeAll()
        {
            if(!IsConnected)
            {
                return;
            }

            multiplexer.GetSubscriber().UnsubscribeAll();
        }

        public Task<long> PublishAsync(string channel, string value)
        {
            if(!IsConnected)
            {
                return TaskCache.FromResult(0L);
            }

            return multiplexer.GetSubscriber().PublishAsync(RedisChannel.Literal(channel), value);

        }
        public Task<KeyValuePair<string, string>[]> GetConfigAsync()
        {
            if(!IsConnected)
            {
                return TaskCache.FromResult(new KeyValuePair<string, string>[] { });
            }

            var server = GetServer();
            return server.ConfigGetAsync();

        }

        protected IServer GetServer()
        {
            var endpoints = multiplexer.GetEndPoints();
            IServer result = null;
            foreach(var endpoint in endpoints)
            {
                var server = multiplexer.GetServer(endpoint);
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

        public abstract bool Connect();

        public abstract void Disconnect();
    }
}
