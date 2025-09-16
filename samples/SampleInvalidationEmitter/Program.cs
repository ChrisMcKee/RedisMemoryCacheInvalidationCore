using System;
using System.Runtime.Caching;
using System.Threading.Tasks;
using RedisMemoryCacheInvalidation;

Console.WriteLine("Simple Invalidation Emitter");

// InvalidationManager.Configure("localhost:6379", new InvalidationSettings());

InvalidationManager.Configure("localhost:6379",
    new InvalidationSettings
    {
        InvalidationStrategy = InvalidationStrategyType.All,
        InvalidationCallback = s =>
        {
            Console.WriteLine("RedisCacheInvalidation : Invalidating {0}, Count: {1}", s, MemoryCache.Default.GetCount());
        },
        TargetCache = MemoryCache.Default
    }
);

Console.WriteLine("IsConnected : " + InvalidationManager.IsConnected);

for(var i = 0; i < 3; i++)
{
    var id = Guid.NewGuid().ToString("N");
    Console.WriteLine(id);
    MemoryCache.Default.Set(id, "test", new CacheItemPolicy());
}

Console.WriteLine("Memory Count before invalidation: " + MemoryCache.Default.GetCount());
Console.WriteLine("enter a key to send invalidation (default is 'mynotifmessage'): ");
var key = Console.ReadLine();
var task = await InvalidationManager.InvalidateAsync(string.IsNullOrEmpty(key) ? "mynotifmessage" : key);
Console.WriteLine("message send to {0} clients", task);
while(true)
{
    await Task.Delay(TimeSpan.FromSeconds(5));
    Console.WriteLine("Memory Count after invalidation: " + MemoryCache.Default.GetCount());
}
