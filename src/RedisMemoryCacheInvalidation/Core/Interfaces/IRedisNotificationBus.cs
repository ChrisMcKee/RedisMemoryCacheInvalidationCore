using System.Runtime.Caching;
using System.Threading.Tasks;
using RedisMemoryCacheInvalidation.Core;
using RedisMemoryCacheInvalidation.Redis;

namespace RedisMemoryCacheInvalidation
{
    public interface IRedisNotificationBus
    {
        IRedisConnection Connection { get; }
        InvalidationStrategyType InvalidationStrategy { get; }
        MemoryCache LocalCache { get; }
        INotificationManager<string> Notifier { get; }
        Task<long> NotifyAsync(string key);
        void Start();
        void Stop();
    }
}
