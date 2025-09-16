using System;
using System.Threading.Tasks;
using StackExchange.Redis;
using Testcontainers.Redis;
using Xunit;

namespace RedisMemoryCacheInvalidation.Tests.Fixtures;

public class RedisServerFixture : IAsyncLifetime
{
    private readonly RedisContainer _redisContainer;
    private ConnectionMultiplexer _mux;
    private string _redisEndpoint;

    public RedisServerFixture()
    {
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7-alpine")
            .WithPortBinding(6379, true)
            .Build();
    }

    public async Task InitializeAsync()
    {
        await _redisContainer.StartAsync();
        
        _redisEndpoint = _redisContainer.GetConnectionString();
        _mux = await ConnectionMultiplexer.ConnectAsync(new ConfigurationOptions 
        { 
            AllowAdmin = true, 
            AbortOnConnectFail = false, 
            EndPoints = { _redisEndpoint }
        });
        
        // Configure keyspace notifications
        var server = _mux.GetServer(_redisEndpoint);
        await server.ConfigSetAsync("notify-keyspace-events", "KEA");

        // Test connection
        var db = _mux.GetDatabase();
        await db.StringSetAsync("key", "value");
        var actualValue = await db.StringGetAsync("key");
    }

    public async Task DisposeAsync()
    {
        if (_mux != null && _mux.IsConnected)
            await _mux.CloseAsync();
        
        await _redisContainer.DisposeAsync();
    }

    public IDatabase GetDatabase(int db)
    {
        return _mux.GetDatabase(db);
    }

    public string GetEndpoint()
    {
        return _redisEndpoint;
    }

    public ISubscriber GetSubscriber()
    {
        return _mux.GetSubscriber();
    }

    public void Reset()
    {
        var server = _mux.GetServer(_redisEndpoint);
        server.FlushAllDatabases();
    }
}