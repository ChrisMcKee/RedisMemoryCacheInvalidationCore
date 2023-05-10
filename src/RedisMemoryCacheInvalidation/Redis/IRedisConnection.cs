using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace RedisMemoryCacheInvalidation.Redis
{
    public interface IRedisConnection
    {
        bool IsConnected { get; }
        bool Connect();
        void Disconnect();
        Task<KeyValuePair<string, string>[]> GetConfigAsync();
        void Subscribe(string channel, Action<RedisChannel, RedisValue> handler);
        void UnsubscribeAll();
        Task<long> PublishAsync(string channel, string value);
    }
}
