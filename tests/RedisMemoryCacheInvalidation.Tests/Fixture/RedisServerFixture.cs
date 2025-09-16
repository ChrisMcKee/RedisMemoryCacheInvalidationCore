using System;
using System.Threading;
using StackExchange.Redis;

namespace RedisMemoryCacheInvalidation.Tests.Fixtures;

public class RedisServerFixture : IDisposable
{
    private static RedisInside.Redis Redis;
    private static string RedisEndpoint;
    private readonly ConnectionMultiplexer _mux;

    public RedisServerFixture()
    {
        Redis = new RedisInside.Redis();
        Thread.Sleep(100);
        _mux = ConnectionMultiplexer.Connect(new ConfigurationOptions { AllowAdmin = true, AbortOnConnectFail = false, EndPoints = { Redis.Endpoint } });
        RedisEndpoint = Redis.Endpoint.ToString();
        _mux.GetServer(Redis.Endpoint.ToString() ?? string.Empty).ConfigSet("notify-keyspace-events", "KEA");

        _mux.GetDatabase().StringSetAsync("key", "value");
        var actualValue = _mux.GetDatabase().StringGetAsync("key"); ;
    }

    public static bool IsRunning => Redis != null;

    public void Dispose()
    {
        if(_mux != null && _mux.IsConnected)
            _mux.Close(false);
        Redis.Dispose();
    }

    public IDatabase GetDatabase(int db)
    {
        return _mux.GetDatabase(db);
    }
    public string GetEndpoint()
    {
        return RedisEndpoint;
    }

    public ISubscriber GetSubscriber()
    {
        return _mux.GetSubscriber();
    }

    public void Reset()
    {
        _mux.GetServer(Redis.Endpoint.ToString() ?? string.Empty).FlushAllDatabases();
    }
}